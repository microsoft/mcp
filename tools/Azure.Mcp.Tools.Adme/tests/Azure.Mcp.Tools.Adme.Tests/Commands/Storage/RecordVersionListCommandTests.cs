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

public sealed class RecordVersionListCommandTests : CommandUnitTestsBase<RecordVersionListCommand, IStorageService>
{
    private const string RecordId = "opendes:master-data--Well:W-99";

    [Fact]
    public async Task Execute_WithId_ReturnsVersions()
    {
        Service.ListRecordVersionsAsync(
                TestConstants.Endpoint, TestConstants.DataPartition, RecordId, TestConstants.Tenant,
                Arg.Any<CancellationToken>())
            .Returns(new RecordVersionsResponse { RecordId = RecordId, Versions = [1L, 2L] });

        var response = await ExecuteCommandAsync(
            "--endpoint", TestConstants.Endpoint,
            "--data-partition", TestConstants.DataPartition,
            "--id", RecordId,
            "--tenant", TestConstants.Tenant);

        var result = ValidateAndDeserializeResponse(response, AdmeJsonContext.Default.RecordVersionsResponse);
        Assert.Equal(RecordId, result.RecordId);
        Assert.Equal([1L, 2L], result.Versions);
    }

    [Theory]
    [InlineData("--id", "")]
    [InlineData("--id", "opendes:work-product-component--SeismicBinGrid:grid-1:")]
    [InlineData("--endpoint", "https://example.com")]
    [InlineData("--data-partition", " ")]
    public async Task Execute_WithInvalidOption_DoesNotCallService(string option, string value)
    {
        var response = await ExecuteCommandAsync(
            "--endpoint", option == "--endpoint" ? value : TestConstants.Endpoint,
            "--data-partition", option == "--data-partition" ? value : TestConstants.DataPartition,
            "--id", option == "--id" ? value : RecordId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceiveWithAnyArgs().ListRecordVersionsAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }
}
