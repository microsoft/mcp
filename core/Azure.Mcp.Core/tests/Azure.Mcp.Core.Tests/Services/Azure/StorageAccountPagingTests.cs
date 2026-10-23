// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public sealed class StorageAccountPagingTests()
{
    [Fact]
    public async Task AccountGet_ResumesSerializedResponseOnFreshInstance()
    {
        JsonElement continuationArguments;
        string commandId;
        using (var firstHandler = new ResourceGraphPagingHttpMessageHandler(_ => ResourceResponse("first", true)))
        {
            var command = CreateCommand(ResourceGraphPagingTestFactory.CreateAzureService(firstHandler, "first-identity"));
            commandId = command.Id;
            var response = await command.ExecuteAsync(
                new(), command.GetCommand().Parse("--subscription subscription-alias --resource-group TEST-RG --page-size 1"),
                TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            var json = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
            Assert.Single(json.GetProperty("accounts").EnumerateArray());
            Assert.True(json.GetProperty("areResultsTruncated").GetBoolean());
            var paging = json.GetProperty("paging");
            Assert.Equal(command.Id, paging.GetProperty("commandId").GetString());
            continuationArguments = paging.GetProperty("continuationArguments").Clone();
            Assert.Equal(ResourceGraphPagingTestFactory.Subscription, continuationArguments.GetProperty("subscription").GetString());
            Assert.Equal(ResourceGraphPagingTestFactory.Tenant, continuationArguments.GetProperty("tenant").GetString());
            Assert.Equal("TEST-RG", continuationArguments.GetProperty("resource-group").GetString());
            Assert.Equal(1, continuationArguments.GetProperty("page-size").GetInt32());
            Assert.NotNull(command.ResultTypeInfo);
            var schema = OptionSchemaGenerator.CreateOutputSchema(command.ResultTypeInfo);
            Assert.NotNull(schema["properties"]?["paging"]);
        }

        using var secondHandler = new ResourceGraphPagingHttpMessageHandler(request =>
        {
            Assert.Equal("native-next", request.GetProperty("options").GetProperty("$skipToken").GetString());
            Assert.Equal(ResourceGraphPagingTestFactory.Subscription, request.GetProperty("subscriptions")[0].GetString());
            return ResourceResponse("second", false);
        });
        var secondCommand = CreateCommand(ResourceGraphPagingTestFactory.CreateAzureService(secondHandler, "second-identity"));
        Assert.Equal(commandId, secondCommand.Id);
        Assert.True(secondCommand.GetCommand().TryParseFromDictionary(
            continuationArguments.EnumerateObject().ToDictionary(property => property.Name, property => property.Value),
            out var parsed, out var errors), string.Join('\n', errors));

        var next = await secondCommand.ExecuteAsync(new(), parsed!, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, next.Status);
        var nextJson = JsonSerializer.SerializeToElement(next, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.Equal("second", Assert.Single(nextJson.GetProperty("accounts").EnumerateArray()).GetProperty("name").GetString());
        Assert.False(nextJson.TryGetProperty("areResultsTruncated", out var truncated) && truncated.GetBoolean());
        Assert.Equal(1, nextJson.GetProperty("paging").GetProperty("pageSize").GetInt32());
        Assert.False(nextJson.GetProperty("paging").TryGetProperty("continuationToken", out _));
        Assert.False(nextJson.GetProperty("paging").TryGetProperty("continuationArguments", out _));
        Assert.Equal("Bearer second-identity", Assert.Single(secondHandler.AuthorizationHeaders));
    }

    [Fact]
    public async Task AccountGet_UsesFiftyItemDefaultWithoutPagingArguments()
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ =>
            new(HttpStatusCode.OK) { Content = new StringContent("""{"data":[],"resultTruncated":false}""") });
        var command = CreateCommand(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var response = await command.ExecuteAsync(
            new(), command.GetCommand().Parse("--subscription subscription-alias"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(50, Assert.Single(handler.Requests).GetProperty("options").GetProperty("$top").GetInt32());
        var results = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.Equal(50, results.GetProperty("paging").GetProperty("pageSize").GetInt32());
    }

    private static IBaseCommand CreateCommand(IAzureService azureService)
    {
        var resolver = Substitute.For<ISubscriptionResolver>();
        resolver.ResolveSubscription(Arg.Any<string?>()).Returns(call => call.ArgAt<string?>(0) ?? "different-machine-default");
        return new AccountGetCommand(NullLogger<AccountGetCommand>.Instance, new StorageService(azureService), resolver);
    }

    private static HttpResponseMessage ResourceResponse(string name, bool hasMore)
    {
        var resource = new JsonObject
        {
            ["id"] = $"/subscriptions/{ResourceGraphPagingTestFactory.Subscription}/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/{name}",
            ["name"] = name,
            ["type"] = "Microsoft.Storage/storageAccounts",
            ["location"] = "eastus",
            ["resourceGroup"] = "test-rg",
            ["kind"] = "StorageV2",
            ["sku"] = new JsonObject { ["name"] = "Standard_LRS", ["tier"] = "Standard" },
            ["properties"] = new JsonObject
            {
                ["provisioningState"] = "Succeeded"
            }
        };
        var body = new JsonObject
        {
            ["data"] = new JsonArray(resource),
            ["resultTruncated"] = hasMore
        };
        if (hasMore)
        {
            body["$skipToken"] = "native-next";
        }
        return new(HttpStatusCode.OK) { Content = new StringContent(body.ToJsonString()) };
    }
}
