// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Identity;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Schema;

public sealed class SchemaListCommandTests : CommandUnitTestsBase<SchemaListCommand, ISchemaService>
{
    [Fact]
    public async Task Execute_WithFilters_ForwardsRequestAndReturnsResponse()
    {
        var expected = new SchemaListResponse
        {
            SchemaInfos =
            [
                new SchemaInfo
                {
                    SchemaIdentity = new SchemaIdentity { Id = TestConstants.WellKind },
                    Status = "PUBLISHED",
                    Scope = "SHARED",
                    SupersededBy = new SchemaIdentity { Id = TestConstants.SupersedingWellKind },
                }
            ],
            Offset = 2,
            Count = 1,
            TotalCount = 3,
        };
        Service.ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            TestConstants.WellAuthority,
            TestConstants.WellSource,
            TestConstants.WellEntityType,
            SchemaStatus.PUBLISHED,
            SchemaScope.SHARED,
            1,
            0,
            0,
            true,
            2,
            25,
            Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SchemaListResponse>(expected, "test-correlation-id"));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--tenant", TestConstants.Tenant,
            "--authority", TestConstants.WellAuthority,
            "--source", TestConstants.WellSource,
            "--entity-type", TestConstants.WellEntityType,
            "--status", "PUBLISHED",
            "--scope", "SHARED",
            "--schema-version-major", "1",
            "--schema-version-minor", "0",
            "--schema-version-patch", "0",
            "--latest-version",
            "--offset", "2",
            "--limit", "25");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSchemaListResponse);
        Assert.Equal(expected.Offset, result.Result.Offset);
        Assert.Equal(expected.Count, result.Result.Count);
        Assert.Equal(expected.TotalCount, result.Result.TotalCount);
        Assert.Equal(expected.SchemaInfos.Single().SchemaIdentity?.Id, result.Result.SchemaInfos.Single().SchemaIdentity?.Id);
        Assert.Equal(expected.SchemaInfos.Single().SupersededBy?.Id, result.Result.SchemaInfos.Single().SupersededBy?.Id);
        Assert.Equal("test-correlation-id", result.CorrelationId);
        await Service.Received(1).ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            TestConstants.WellAuthority,
            TestConstants.WellSource,
            TestConstants.WellEntityType,
            SchemaStatus.PUBLISHED,
            SchemaScope.SHARED,
            1,
            0,
            0,
            true,
            2,
            25,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithNoFilters_PreservesNullOptionalValues()
    {
        Service.ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<SchemaListResponse>(new SchemaListResponse { SchemaInfos = [] }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseSchemaListResponse);
        Assert.Empty(result.Result.SchemaInfos);
        Assert.Null(result.CorrelationId);
        await Service.Received(1).ListSchemasAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            Arg.Any<CancellationToken>());
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
        Service.ListSchemasAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<SchemaStatus?>(),
                Arg.Any<SchemaScope?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<bool>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("az login", response.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        var arguments = omitEndpoint
            ? new[] { "--data-partition", TestConstants.DataPartition }
            : new[] { "--endpoint", TestConstants.Endpoint };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListSchemasAsync(
            default!, default!, default, default, default, default, default, default,
            default, default, default, default, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--endpoint", "ftp://sample.energy.azure.com")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidTarget_DoesNotCallService(string option, string value)
    {
        var endpoint = option == "--endpoint" ? value : TestConstants.Endpoint;
        var dataPartition = option == "--data-partition" ? value : TestConstants.DataPartition;

        var response = await ExecuteCommandAsync(
            "--endpoint", endpoint,
            "--data-partition", dataPartition);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListSchemasAsync(
            default!, default!, default, default, default, default, default, default,
            default, default, default, default, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--latest-version --schema-version-minor 0")]
    [InlineData("--latest-version --schema-version-major 1 --schema-version-patch 0")]
    [InlineData("--offset -1")]
    [InlineData("--limit -1")]
    public async Task Execute_WithApiRejectedOptions_DoesNotCallService(string invalidArguments)
    {
        var response = await ExecuteCommandAsync(
            [
                "--endpoint", TestConstants.Endpoint,
                "--data-partition", TestConstants.DataPartition,
                .. invalidArguments.Split(' '),
            ]);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListSchemasAsync(
            default!, default!, default, default, default, default, default, default,
            default, default, default, default, default, default, TestContext.Current.CancellationToken);
    }
}
