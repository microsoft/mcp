// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
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
                    SchemaIdentity = new SchemaIdentity { Id = "osdu:wks:master-data--Well:1.0.0" },
                    Status = "PUBLISHED",
                    Scope = "SHARED",
                }
            ],
            Offset = 2,
            Count = 1,
            TotalCount = 3,
        };
        Service.ListSchemasAsync(
                "https://sample.energy.azure.com",
                "opendes",
                "osdu",
                "wks",
                "master-data--Well",
                SchemaStatus.PUBLISHED,
                SchemaScope.SHARED,
                1,
                0,
                0,
                true,
                2,
                25,
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes",
            "--authority", "osdu",
            "--source", "wks",
            "--entity-type", "master-data--Well",
            "--status", "PUBLISHED",
            "--scope", "SHARED",
            "--schema-version-major", "1",
            "--schema-version-minor", "0",
            "--schema-version-patch", "0",
            "--latest-version",
            "--offset", "2",
            "--limit", "25");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.SchemaListResponse);
        Assert.Equal(expected.Offset, result.Offset);
        Assert.Equal(expected.Count, result.Count);
        Assert.Equal(expected.TotalCount, result.TotalCount);
        Assert.Equal(expected.SchemaInfos.Single().SchemaIdentity?.Id, result.SchemaInfos.Single().SchemaIdentity?.Id);
        await Service.Received(1).ListSchemasAsync(
            "https://sample.energy.azure.com",
            "opendes",
            "osdu",
            "wks",
            "master-data--Well",
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
    public async Task Execute_WithNoFilters_AppliesDefaults()
    {
        Service.ListSchemasAsync(
                "https://sample.energy.azure.com",
                "opendes",
                null,
                null,
                null,
                SchemaStatus.PUBLISHED,
                null,
                null,
                null,
                null,
                false,
                0,
                100,
                Arg.Any<CancellationToken>())
            .Returns(new SchemaListResponse { SchemaInfos = [] });

        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.SchemaListResponse);
        Assert.Empty(result.SchemaInfos);
        await Service.Received(1).ListSchemasAsync(
            "https://sample.energy.azure.com",
            "opendes",
            null,
            null,
            null,
            SchemaStatus.PUBLISHED,
            null,
            null,
            null,
            null,
            false,
            0,
            100,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithoutRequiredTargetOption_DoesNotCallService(bool omitEndpoint)
    {
        var arguments = omitEndpoint
            ? new[] { "--data-partition", "opendes" }
            : new[] { "--endpoint", "https://sample.energy.azure.com" };

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListSchemasAsync(
            default!, default!, default, default, default, default, default, default,
            default, default, default, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("--offset", "-1")]
    [InlineData("--limit", "0")]
    [InlineData("--limit", "1001")]
    [InlineData("--schema-version-major", "-1")]
    [InlineData("--schema-version-minor", "1")]
    [InlineData("--schema-version-patch", "1")]
    public async Task Execute_WithInvalidPagingOrVersion_ReturnsValidationError(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", "https://sample.energy.azure.com",
            "--data-partition", "opendes",
            option, value);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListSchemasAsync(
            default!, default!, default, default, default, default, default, default,
            default, default, default, default, default, TestContext.Current.CancellationToken);
    }
}
