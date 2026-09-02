// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Identity;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Schema;

public sealed class SchemaGetCommandTests : CommandUnitTestsBase<SchemaGetCommand, ISchemaService>
{
    [Fact]
    public async Task Execute_WithKind_ForwardsRequestAndReturnsSchema()
    {
        var schema = JsonDocument.Parse("""{"title":"Well"}""").RootElement.Clone();
        Service.GetSchemaAsync(
                TestConstants.Endpoint,
                TestConstants.DataPartition,
                TestConstants.WellKind,
                TestConstants.Tenant,
                Arg.Any<CancellationToken>())
            .Returns(schema);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.JsonElement);
        Assert.Equal("Well", result.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WhenAuthenticationFails_ReturnsUnauthorizedWithSignInGuidance(
        bool credentialUnavailable)
    {
        var exception = credentialUnavailable
            ? new CredentialUnavailableException("No credential available.")
            : new AuthenticationFailedException("Token acquisition failed.");
        Service.GetSchemaAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("az login", response.Message);
    }

    [Fact]
    public async Task Execute_WithoutKind_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        await Service.DidNotReceive().GetSchemaAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        var arguments = omitEndpoint
            ? new[] { "--data-partition", TestConstants.DataPartition, "--kind", TestConstants.WellKind }
            : new[] { "--endpoint", TestConstants.Endpoint, "--kind", TestConstants.WellKind };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetSchemaAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--endpoint", "not-a-uri")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidTarget_DoesNotCallService(string option, string value)
    {
        var endpoint = option == "--endpoint" ? value : TestConstants.Endpoint;
        var dataPartition = option == "--data-partition" ? value : TestConstants.DataPartition;

        var response = await ExecuteCommandAsync(
            "--endpoint", endpoint,
            "--data-partition", dataPartition,
            "--kind", TestConstants.WellKind);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetSchemaAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }
}
