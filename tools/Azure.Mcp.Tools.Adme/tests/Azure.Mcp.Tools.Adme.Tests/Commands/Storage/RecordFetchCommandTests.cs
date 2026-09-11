// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Adme.Commands.Storage;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Commands.Storage;

public sealed class RecordFetchCommandTests : CommandUnitTestsBase<RecordFetchCommand, IStorageService>
{
    private const string RecordId = "opendes:master-data--Well:W-99";

    [Fact]
    public async Task Execute_WithIds_FetchesFullRecords()
    {
        Service.FetchRecordsAsync(
                TestConstants.Endpoint, TestConstants.DataPartition,
                Arg.Is<IReadOnlyList<string>>(ids => ids.SequenceEqual(new[] { RecordId })),
                Arg.Is<IReadOnlyList<string>?>(attributes => attributes == null), false,
                TestConstants.Tenant, Arg.Any<CancellationToken>())
            .Returns(new FetchRecordsResponse { Records = [new StorageRecord { Id = RecordId }] });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--ids", RecordId,
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.FetchRecordsResponse);
        Assert.Equal(RecordId, Assert.Single(result.Records).Id);
    }

    [Fact]
    public async Task Execute_WithAttributes_ForwardsProjection()
    {
        Service.FetchRecordsAsync(
                TestConstants.Endpoint, TestConstants.DataPartition,
                Arg.Is<IReadOnlyList<string>>(ids => ids.SequenceEqual(new[] { RecordId })),
                Arg.Is<IReadOnlyList<string>>(attributes => attributes.SequenceEqual(new[] { "data.Name" })),
                false, null, Arg.Any<CancellationToken>())
            .Returns(new FetchRecordsResponse { Records = [] });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--ids", RecordId,
            "--attributes", "data.Name");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.FetchRecordsResponse);
        Assert.Empty(result.Records);
    }

    [Fact]
    public async Task Execute_WithFrameOfReference_RequestsConversion()
    {
        Service.FetchRecordsAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, Arg.Any<IReadOnlyList<string>>(),
                Arg.Is<IReadOnlyList<string>?>(attributes => attributes == null), true, null,
                Arg.Any<CancellationToken>())
            .Returns(new FetchRecordsResponse
            {
                Records = [new StorageRecord { Id = RecordId }],
                ConversionStatuses = [new ConversionStatus { Id = RecordId, Status = "NO_FRAME_OF_REFERENCE" }],
            });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--ids", RecordId,
            "--frame-of-reference");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.FetchRecordsResponse);
        Assert.Equal("NO_FRAME_OF_REFERENCE", Assert.Single(result.ConversionStatuses!).Status);
    }

    [Fact]
    public async Task Execute_WithoutIds_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().FetchRecordsAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithTooManyIds_DoesNotCallService()
    {
        var arguments = new List<string>
        {
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--ids",
        };
        arguments.AddRange(Enumerable.Range(0, 21).Select(index => $"opendes:master-data--Well:W-{index}"));

        var response = await ExecuteCommandAsync([.. arguments]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().FetchRecordsAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithAttributesAndFrameOfReference_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--ids", RecordId,
            "--attributes", "data.Name",
            "--frame-of-reference");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().FetchRecordsAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ValidateOptions_WithEmptyAttributes_ReturnsClearError()
    {
        var options = new RecordFetchOptions
        {
            Endpoint = TestConstants.Endpoint,
            DataPartition = TestConstants.DataPartition,
            Ids = [RecordId],
            Attributes = [],
        };
        var validationResult = new ValidationResult();

        Command.ValidateOptions(options, validationResult);

        Assert.Contains(
            "--attributes cannot be empty or contain blank fields when specified.",
            validationResult.Errors);
    }

    [Fact]
    public void ValidateOptions_WithBlankAttribute_ReturnsClearError()
    {
        var options = new RecordFetchOptions
        {
            Endpoint = TestConstants.Endpoint,
            DataPartition = TestConstants.DataPartition,
            Ids = [RecordId],
            Attributes = ["data.Name", " "],
        };
        var validationResult = new ValidationResult();

        Command.ValidateOptions(options, validationResult);

        Assert.Contains(
            "--attributes cannot be empty or contain blank fields when specified.",
            validationResult.Errors);
    }
}
