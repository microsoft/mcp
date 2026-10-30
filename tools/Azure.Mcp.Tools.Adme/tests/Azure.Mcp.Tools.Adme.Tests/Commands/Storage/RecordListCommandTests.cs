// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Adme.Commands.Storage;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Models;
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
            "prev-cursor", TestConstants.Tenant, null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<QueryRecordsResponse>(
                new QueryRecordsResponse { Cursor = "next-cursor", Results = [RecordId] },
                "test-correlation-id"));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--limit", "25",
            "--cursor", "prev-cursor",
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseRecordListResult);
        Assert.Equal("next-cursor", result.NextCursor);
        Assert.Equal(RecordId, Assert.Single(result.Result.Results));
        Assert.Equal("test-correlation-id", result.CorrelationId);
        var payload = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.Equal("next-cursor", payload.GetProperty("nextCursor").GetString());
        Assert.Equal(["results"], payload.GetProperty("result").EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task Execute_WithoutPagingOptions_AppliesDefaultLimit()
    {
        Service.QueryRecordsByKindAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 10,
            null, null, null, Arg.Any<CancellationToken>())
            .Returns(new AdmeResponse<QueryRecordsResponse>(new QueryRecordsResponse { Results = [] }, null));

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.AdmeResponseRecordListResult);
        Assert.Empty(result.Result.Results);
        Assert.Null(result.NextCursor);
        var payload = JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.False(payload.TryGetProperty("nextCursor", out _));
        Assert.Equal(["results"], payload.GetProperty("result").EnumerateObject().Select(property => property.Name));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_ContinuesWithNextCursorIncludingEmptyPages(bool emptyFirstPage)
    {
        const string cursor = " opaque+/=%2F== ";
        Service.QueryRecordsByKindAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 1,
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(
                new AdmeResponse<QueryRecordsResponse>(
                    new QueryRecordsResponse { Results = emptyFirstPage ? [] : [RecordId], Cursor = cursor },
                    "first-correlation"),
                new AdmeResponse<QueryRecordsResponse>(
                    new QueryRecordsResponse { Results = ["opendes:master-data--Well:W-100"] },
                    "final-correlation"));
        string[] arguments =
        [
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--kind", TestConstants.WellKind,
            "--limit", "1",
        ];

        var firstResponse = await ExecuteCommandAsync(arguments);
        var first = ValidateAndDeserializeResponse(firstResponse, AdmeJsonContext.Default.AdmeResponseRecordListResult);
        Assert.Equal(cursor, first.NextCursor);
        Assert.Equal(emptyFirstPage ? 0 : 1, first.Result.Results.Count);
        Assert.Equal("first-correlation", first.CorrelationId);
        var firstPayload = JsonSerializer.SerializeToElement(firstResponse, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.Equal(cursor, firstPayload.GetProperty("nextCursor").GetString());
        Assert.Equal(["results"], firstPayload.GetProperty("result").EnumerateObject().Select(property => property.Name));

        var finalResponse = await ExecuteCommandAsync([.. arguments, "--cursor", first.NextCursor!]);
        var final = ValidateAndDeserializeResponse(finalResponse, AdmeJsonContext.Default.AdmeResponseRecordListResult);
        Assert.Equal("opendes:master-data--Well:W-100", Assert.Single(final.Result.Results));
        Assert.Null(final.NextCursor);
        Assert.Equal("final-correlation", final.CorrelationId);
        var finalPayload = JsonSerializer.SerializeToElement(finalResponse, ModelsJsonContext.Default.CommandResponse).GetProperty("results");
        Assert.False(finalPayload.TryGetProperty("nextCursor", out _));
        Assert.Equal(["results"], finalPayload.GetProperty("result").EnumerateObject().Select(property => property.Name));

        await Service.Received(1).QueryRecordsByKindAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 1,
            null, null, null, Arg.Any<CancellationToken>());
        await Service.Received(1).QueryRecordsByKindAsync(
            TestConstants.Endpoint, TestConstants.DataPartition, TestConstants.WellKind, 1,
            cursor, null, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--kind", "")]
    [InlineData("--kind", "opendes:wks:master-data--Well")]
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
            default!, default!, default!, default, default, default, default,
            TestContext.Current.CancellationToken);
    }
}
