// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.Container;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests.Container;

public class ContainerSchemaInferCommandTests : SubscriptionCommandUnitTestsBase<ContainerSchemaInferCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("infer", Command.Name);

    [Fact]
    public void Metadata_IsReadOnly()
    {
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_OnSuccess()
    {
        var properties = new List<SchemaProperty>
        {
            new("id", "string", 5, 5),
            new("name", "string", 4, 5),
        };
        Cosmos.Models.ContainerSchema schema = new(5, properties);

        Service.GetApproximateSchema(
            Arg.Is("acct"),
            Arg.Is("db"),
            Arg.Is("c"),
            Arg.Is(5),
            Arg.Is("sub"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(schema);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--sample-size", "5");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ContainerSchemaInferCommandResult);
        Assert.Equal(5, result.SampleSize);
        Assert.Equal(2, result.Properties.Count);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("21")]
    public async Task ExecuteAsync_RejectsOutOfRangeSampleSize(string sampleSize)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--sample-size", sampleSize);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("sample-size", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesServiceErrors()
    {
        Service.GetApproximateSchema(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(),
            Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
