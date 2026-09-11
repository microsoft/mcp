// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Adme.Commands.Storage;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Storage;

public sealed class RecordListCommandTests : CommandUnitTestsBase<RecordListCommand, IStorageService>
{
    private const string RecordId = "opendes:master-data--Well:W-99";

    [Fact]
    public async Task Execute_WithPagingOptions_ForwardsRequestAndReturnsResponse()
    {
        Service.QueryRecordsByKindAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 25,
                "prev-cursor", TestConstants.Tenant, Arg.Any<CancellationToken>())
            .Returns(new QueryRecordsResponse { Cursor = "next-cursor", Results = [RecordId] });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--limit", "25",
            "--cursor", "prev-cursor",
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.QueryRecordsResponse);
        Assert.Equal("next-cursor", result.Cursor);
        Assert.Equal(RecordId, Assert.Single(result.Results));
    }

    [Fact]
    public async Task Execute_WithoutPagingOptions_AppliesDefaultLimit()
    {
        Service.QueryRecordsByKindAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 10,
                null, null, Arg.Any<CancellationToken>())
            .Returns(new QueryRecordsResponse { Results = [] });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.QueryRecordsResponse);
        Assert.Empty(result.Results);
    }

    [Theory]
    [InlineData("--kind", "")]
    [InlineData("--kind", "opendes:wks:master-data--Well")]
    [InlineData("--kind", "opendes:wks:master-data--Well:*")]
    [InlineData("--kind", "opendes:wks:master-data--Well:1.0")]
    [InlineData("--limit", "0")]
    [InlineData("--limit", "101")]
    [InlineData("--endpoint", "ftp://sample.energy.azure.com")]
    public async Task Execute_WithInvalidOption_DoesNotCallService(string option, string value)
    {
        var arguments = new List<string>
        {
            "--endpoint", option == "--endpoint" ? value : TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", option == "--kind" ? value : TestConstants.WellKind,
        };
        if (option == "--limit")
        {
            arguments.AddRange(["--limit", value]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().QueryRecordsByKindAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }
}
