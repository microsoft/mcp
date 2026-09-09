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

public sealed class RecordGetCommandTests : CommandUnitTestsBase<RecordGetCommand, IStorageService>
{
    private const string RecordId = "opendes:master-data--Well:W-99";

    [Fact]
    public async Task Execute_WithIdOnly_RequestsLatestVersion()
    {
        Service.GetRecordAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, RecordId, null,
                Arg.Is<IReadOnlyList<string>?>(attributes => attributes == null), TestConstants.Tenant,
                Arg.Any<CancellationToken>())
            .Returns(new StorageRecord { Id = RecordId, Kind = TestConstants.WellKind });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--id", RecordId,
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.StorageRecord);
        Assert.Equal(RecordId, result.Id);
    }

    [Fact]
    public async Task Execute_WithVersionAndAttributes_ForwardsBothOptions()
    {
        const long version = 1704779151123456;
        Service.GetRecordAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, RecordId, version,
                Arg.Is<IReadOnlyList<string>>(attributes => attributes.SequenceEqual(new[] { "data.Name" })),
                null, Arg.Any<CancellationToken>())
            .Returns(new StorageRecord { Id = RecordId, Version = version });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--id", RecordId,
            "--version", version.ToString(),
            "--attributes", "data.Name");

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.StorageRecord);
        Assert.Equal(version, result.Version);
    }

    [Theory]
    [InlineData("--id", "")]
    [InlineData("--id", "opendes:work-product-component--SeismicBinGrid:grid-1:")]
    [InlineData("--id", "opendes::work-product-component--SeismicBinGrid:grid-1")]
    [InlineData("--id", "opendes:master-data-Well:W-99")]
    [InlineData("--version", "0")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidOption_DoesNotCallService(string option, string value)
    {
        var arguments = new List<string>
        {
            "--endpoint", option == "--endpoint" ? value : TestConstants.Endpoint,
            "--data-partition", option == "--data-partition" ? value : TestConstants.DataPartition,
            "--id", option == "--id" ? value : RecordId,
        };
        if (option == "--version")
        {
            arguments.AddRange(["--version", value]);
        }

        var response = await ExecuteCommandAsync([.. arguments]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetRecordAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Execute_WithoutId_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().GetRecordAsync(
            default!, default!, default!, default, default, default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ValidateOptions_WithEmptyAttributes_ReturnsClearError()
    {
        var options = new RecordGetOptions
        {
            Endpoint = TestConstants.Endpoint,
            DataPartition = TestConstants.DataPartition,
            Id = RecordId,
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
        var options = new RecordGetOptions
        {
            Endpoint = TestConstants.Endpoint,
            DataPartition = TestConstants.DataPartition,
            Id = RecordId,
            Attributes = ["data.Name", " "],
        };
        var validationResult = new ValidationResult();

        Command.ValidateOptions(options, validationResult);

        Assert.Contains(
            "--attributes cannot be empty or contain blank fields when specified.",
            validationResult.Errors);
    }
}
