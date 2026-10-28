// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Knowledge;

public class KnowledgeBaseRetrieveCommandTests : CommandUnitTestsBase<KnowledgeBaseRetrieveCommand, ISearchService>
{
    [Theory]
    [InlineData(null)]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("Target tenant")]
    public async Task ExecuteAsync_ForwardsTenant(string? tenant)
    {
        Service.RetrieveFromKnowledgeBase("service123", "base1", "hi", null, null, tenant, Arg.Any<CancellationToken>())
            .Returns("{}");
        string[] args = ["--service", "service123", "--knowledge-base", "base1", "--query", "hi"];
        var response = await ExecuteCommandAsync(tenant is null ? args : [.. args, "--tenant", tenant]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--tenant");
        await Service.Received(1).RetrieveFromKnowledgeBase(
            "service123", "base1", "hi", null, null, tenant, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task ExecuteAsync_PreservesServiceStatus(int status)
    {
        Service.RetrieveFromKnowledgeBase("service123", "base1", "hi", null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, "Search request failed"));

        var response = await ExecuteCommandAsync("--service", "service123", "--knowledge-base", "base1", "--query", "hi");

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.Contains("Search request failed", response.Message);
        if (status is 401 or 403)
        {
            Assert.Contains(SearchErrorMessages.Retrieval, response.Message);
        }
        else
        {
            Assert.DoesNotContain(SearchErrorMessages.Retrieval, response.Message);
        }
    }

    [Fact]
    public void ReferenceSourceDataOption_IsAnOptionalNullableBoolean()
    {
        var option = Assert.Single(CommandDefinition.Options,
            option => option.Name == "--include-reference-source-data");

        Assert.False(option.Required);
        Assert.Equal(typeof(bool?), option.ValueType);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithQuery()
    {
        var json = "{\"answer\":\"42\"}";
        Service.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is("life"),
            Arg.Is<List<(string role, string message)>?>(m => m == null),
            Arg.Is<bool?>(value => value == null),
            null,
            Arg.Any<CancellationToken>())
            .Returns(json);

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "life");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.KnowledgeBaseRetrieveCommandResult);
        Assert.Equal(json, result.RetrievalResult);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithMessages()
    {
        var json = "{\"conversation\":true}";
        Service.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is<string?>(q => q == null),
            Arg.Is<List<(string role, string message)>?>(m => m != null && m.Count == 1 && m[0].role == "user"),
            Arg.Is<bool?>(value => value == null),
            null,
            Arg.Any<CancellationToken>())
            .Returns(json);

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--messages", "user:Hello");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("true", false)]
    [InlineData("false", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("true", true)]
    [InlineData("false", true)]
    public async Task ExecuteAsync_PassesReferenceSourceDataOptionToService(string? value, bool useMessages)
    {
        bool? expectedValue = value == null ? null : value.Length == 0 || bool.Parse(value);
        Service.RetrieveFromKnowledgeBase(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<(string role, string message)>?>(),
            Arg.Any<bool?>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns("{}");

        List<string> args = ["--service", "svc", "--knowledge-base", "base1"];
        args.AddRange(useMessages ? ["--messages", "user:Hello"] : ["--query", "Hello"]);
        if (value != null)
        {
            args.Add("--include-reference-source-data");
            if (value.Length > 0)
            {
                args.Add(value);
            }
        }

        var response = await ExecuteCommandAsync([.. args]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RetrieveFromKnowledgeBase(
            "svc",
            "base1",
            useMessages ? null : "Hello",
            Arg.Is<IEnumerable<(string role, string message)>?>(messages =>
                useMessages ? messages != null && messages.Single().role == "user" && messages.Single().message == "Hello" : messages == null),
            expectedValue,
            null,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidReferenceSourceDataOption()
    {
        var response = await ExecuteCommandAsync(
            "--service", "svc", "--knowledge-base", "base1", "--query", "Hello",
            "--include-reference-source-data", "invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("invalid", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMissingQueryAndMessages()
    {
        var response = await ExecuteCommandAsync("--service", "svc", "--knowledge-base", "base1");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --query or at least one --messages", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenHasBothQueryAndMessages()
    {
        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "life",
            "--messages", "user:Hello");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specifying both --query and --messages is not allowed.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMessageFormatInvalid()
    {
        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--messages", "bad-format");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid message format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        Service.RetrieveFromKnowledgeBase(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<List<(string role, string message)>?>(),
            Arg.Any<bool?>(),
            null,
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test failure"));

        var response = await ExecuteCommandAsync(
            "--service", "svc",
            "--knowledge-base", "base1",
            "--query", "hi");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test failure", response.Message);
    }
}
