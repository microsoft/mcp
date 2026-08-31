// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Schema;

public sealed class SchemaGetCommandTests : CommandUnitTestsBase<SchemaGetCommand, ISchemaService>
{
    [Fact]
    public async Task Execute_WithKind_ForwardsRequestAndReturnsSchema()
    {
        const string kind = "osdu:wks:master-data--Well:1.0.0";
        var schema = JsonDocument.Parse("""{"title":"Well"}""").RootElement.Clone();
        Service.GetSchemaAsync(
                "https://sample.energy.azure.com",
                "opendes",
                kind,
                Arg.Any<CancellationToken>())
            .Returns(schema);

        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes",
            "--kind", kind);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.JsonElement);
        Assert.Equal("Well", result.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Execute_WithoutKind_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes");

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        await Service.DidNotReceive().GetSchemaAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        const string kind = "osdu:wks:master-data--Well:1.0.0";
        var arguments = omitEndpoint
            ? new[] { "--data-partition", "opendes", "--kind", kind }
            : new[] { "--endpoint", "https://sample.energy.azure.com", "--kind", kind };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetSchemaAsync(
            default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--endpoint", "not-a-uri")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidTarget_DoesNotCallService(string option, string value)
    {
        const string kind = "osdu:wks:master-data--Well:1.0.0";
        var endpoint = option == "--endpoint" ? value : "https://sample.energy.azure.com";
        var dataPartition = option == "--data-partition" ? value : "opendes";

        var response = await ExecuteCommandAsync(
            "--endpoint", endpoint,
            "--data-partition", dataPartition,
            "--kind", kind);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetSchemaAsync(
            default!, default!, default!, TestContext.Current.CancellationToken);
    }
}
