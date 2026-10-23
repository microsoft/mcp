// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Compute.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests.Disk;

public class DiskTagsServiceTests
{
    private const string Subscription = "22222222-2222-2222-2222-222222222222";

    [Theory]
    [InlineData("env=prod,team=compute")]
    [InlineData("env=prod, team=compute")]
    public async Task CreateDiskAsync_CommaSeparatedTags_SendsEachTag(string tagsOption)
    {
        var tags = new List<JsonElement>();
        using var client = new HttpClient(CreateHandler(HttpMethod.Put, tags));

        await CreateService(client).CreateDiskAsync(
            "disk", "rg", Subscription, location: "eastus", sizeGb: 32, tags: tagsOption,
            cancellationToken: TestContext.Current.CancellationToken);

        AssertTags(Assert.Single(tags));
    }

    [Theory]
    [InlineData("env=prod,team=compute")]
    [InlineData("env=prod, team=compute")]
    public async Task UpdateDiskAsync_CommaSeparatedTags_SendsEachTag(string tagsOption)
    {
        var tags = new List<JsonElement>();
        using var client = new HttpClient(CreateHandler(HttpMethod.Patch, tags));

        await CreateService(client).UpdateDiskAsync(
            "disk", "rg", Subscription, tags: tagsOption,
            cancellationToken: TestContext.Current.CancellationToken);

        AssertTags(Assert.Single(tags));
    }

    private static void AssertTags(JsonElement tags)
    {
        Assert.Equal(2, tags.EnumerateObject().Count());
        Assert.Equal("prod", tags.GetProperty("env").GetString());
        Assert.Equal("compute", tags.GetProperty("team").GetString());
    }

    private static ComputeService CreateService(HttpClient client)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = Substitute.For<IAzureService>();
        azureService.GetClient().Returns(client);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, client, ArmEnvironment.AzurePublicCloud);
        return new ComputeService(azureService, NullLogger<ComputeService>.Instance, Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration()));
    }

    private static HttpMessageHandler CreateHandler(HttpMethod writeMethod, List<JsonElement> sentTags)
    {
        const string resourceGroup = $$"""
            {
              "id": "/subscriptions/{{Subscription}}/resourceGroups/rg",
              "name": "rg",
              "type": "Microsoft.Resources/resourceGroups",
              "location": "eastus",
              "properties": { "provisioningState": "Succeeded" }
            }
            """;
        const string disk = $$"""
            {
              "id": "/subscriptions/{{Subscription}}/resourceGroups/rg/providers/Microsoft.Compute/disks/disk",
              "name": "disk",
              "type": "Microsoft.Compute/disks",
              "location": "eastus",
              "sku": { "name": "Standard_LRS" },
              "properties": { "creationData": { "createOption": "Empty" }, "diskSizeGB": 32, "provisioningState": "Succeeded" }
            }
            """;

        var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            if (request.Method == writeMethod)
            {
                using var body = JsonDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
                sentTags.Add(body.RootElement.GetProperty("tags").Clone());
            }

            var path = request.RequestUri!.AbsolutePath;
            var content = path.EndsWith("/resourceGroups/rg", StringComparison.OrdinalIgnoreCase) ? resourceGroup : disk;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, null, "application/json")
            });
        });
        return handler;
    }
}
