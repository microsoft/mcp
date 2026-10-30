// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public sealed class RecommendationPluralInputTests()
{
    private const string TypeId = "48eda464-1485-4dcf-a674-d0905df5054a";

    [Theory]
    [InlineData("list")]
    [InlineData("summary")]
    [InlineData("prevalidate")]
    public void Options_ExposePluralReadScopesAndPreserveSingleTargetInputs(string operation)
    {
        var command = CreateCommand(operation, Substitute.For<ISubscriptionResolver>());
        var options = command.GetCommand().Options;

        Assert.Equal(typeof(string[]), Assert.Single(options, option => option.Name == "--subscriptions").ValueType);
        Assert.Equal(typeof(string), Assert.Single(options, option => option.Name == "--service-group").ValueType);
        Assert.DoesNotContain(options, option => option.Name == "--subscription" || option.Aliases.Contains("--subscription"));
        if (operation == "prevalidate")
        {
            Assert.Equal(typeof(string), Assert.Single(options, option => option.Name == "--recommendation-type-id").ValueType);
        }
        else
        {
            Assert.Equal(typeof(string[]), Assert.Single(options, option => option.Name == "--recommendation-type-ids").ValueType);
            Assert.Equal(typeof(string[]), Assert.Single(options, option => option.Name == "--resource-types").ValueType);
            Assert.DoesNotContain(options, option => option.Name is "--recommendation-type-id" or "--resource-type");
        }
    }

    [Theory]
    [InlineData("list", "subscription")]
    [InlineData("list", "recommendation-type-id")]
    [InlineData("list", "resource-type")]
    [InlineData("summary", "subscription")]
    [InlineData("summary", "recommendation-type-id")]
    [InlineData("summary", "resource-type")]
    [InlineData("prevalidate", "subscription")]
    public async Task LegacyInputs_FailWithoutUsingTheDefaultSubscription(string operation, string legacyInput)
    {
        var resolver = Substitute.For<ISubscriptionResolver>();
        resolver.ResolveSubscription(null).Returns("default-subscription");
        var command = CreateCommand(operation, resolver);
        using var document = JsonDocument.Parse("\"legacy-value\"");
        var arguments = new Dictionary<string, JsonElement> { [legacyInput] = document.RootElement };

        Assert.False(command.GetCommand().TryParseFromDictionary(arguments, out var parsed, out var error));
        Assert.Null(parsed);
        Assert.Contains(legacyInput, error);

        var cliResult = command.GetCommand().Parse([$"--{legacyInput}", "legacy-value"]);
        var response = await command.ExecuteAsync(new CommandContext(), cliResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        resolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
    }

    [Theory]
    [InlineData("list", "{}")]
    [InlineData("list", """{"subscriptions":null}""")]
    [InlineData("list", """{"subscriptions":[]}""")]
    [InlineData("summary", "{}")]
    [InlineData("summary", """{"subscriptions":null}""")]
    [InlineData("summary", """{"subscriptions":[]}""")]
    [InlineData("prevalidate", "{}")]
    [InlineData("prevalidate", """{"subscriptions":null}""")]
    [InlineData("prevalidate", """{"subscriptions":[]}""")]
    public async Task McpEmptyOrOmittedScope_PreservesExistingDefaultBehavior(string operation, string json)
    {
        var resolver = Substitute.For<ISubscriptionResolver>();
        resolver.ResolveSubscription(null).Returns("default-subscription");
        var command = CreateCommand(operation, resolver);
        var response = await ExecuteMcpAsync(command, operation, json);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        resolver.Received(1).ResolveSubscription(null);
    }

    [Theory]
    [InlineData("list", """{"subscriptions":["one"]}""", true)]
    [InlineData("list", """{"subscriptions":["one","ONE","two"]}""", true)]
    [InlineData("summary", """{"subscriptions":["one"]}""", true)]
    [InlineData("summary", """{"subscriptions":["one","two"]}""", true)]
    [InlineData("prevalidate", """{"subscriptions":["one"]}""", true)]
    [InlineData("prevalidate", """{"subscriptions":["one","two"]}""", true)]
    [InlineData("list", """{"subscriptions":[""]}""", false)]
    [InlineData("summary", """{"subscriptions":[""]}""", false)]
    [InlineData("prevalidate", """{"subscriptions":[""]}""", false)]
    [InlineData("list", """{"subscriptions":["one"],"service-group":"group"}""", false)]
    [InlineData("summary", """{"subscriptions":["one"],"service-group":"group"}""", false)]
    [InlineData("prevalidate", """{"subscriptions":["one"],"service-group":"group"}""", false)]
    [InlineData("list", """{"subscriptions":["one"],"recommendation-type-ids":[""]}""", false)]
    [InlineData("summary", """{"subscriptions":["one"],"resource-types":[""]}""", false)]
    [InlineData("list", """{"subscriptions":["one"],"recommendation-type-ids":["48eda464-1485-4dcf-a674-d0905df5054a"],"resource-types":["Microsoft.Compute/disks"],"prioritized":true}""", true)]
    [InlineData("list", """{"subscriptions":["one"],"recommendation-type-ids":["48eda464-1485-4dcf-a674-d0905df5054a","48EDA464-1485-4DCF-A674-D0905DF5054A"],"prioritized":true}""", true)]
    [InlineData("list", """{"subscriptions":["one"],"recommendation-type-ids":["48eda464-1485-4dcf-a674-d0905df5054a","1d70919c-1a4a-4f79-8300-bb576c291e9d"],"prioritized":true}""", false)]
    public async Task McpExplicitArrays_ValidateWithoutDefaultScopeFallback(string operation, string json, bool valid)
    {
        var resolver = Substitute.For<ISubscriptionResolver>();
        resolver.ResolveSubscription(null).Returns("default-subscription");
        var command = CreateCommand(operation, resolver);
        var response = await ExecuteMcpAsync(command, operation, json);

        Assert.Equal(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        resolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
    }

    private static async Task<CommandResponse> ExecuteMcpAsync(IBaseCommand command, string operation, string json)
    {
        using var document = JsonDocument.Parse(json);
        var arguments = document.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value);
        using var typeId = JsonDocument.Parse($"\"{TypeId}\"");
        if (operation == "prevalidate")
        {
            arguments["recommendation-type-id"] = typeId.RootElement;
        }
        Assert.True(command.GetCommand().TryParseFromDictionary(arguments, out var parsed, out var error), error);
        Assert.NotNull(parsed);
        return await command.ExecuteAsync(new CommandContext(), parsed, TestContext.Current.CancellationToken);
    }

    private static IBaseCommand CreateCommand(string operation, ISubscriptionResolver resolver)
    {
        var advisor = Substitute.For<IAdvisorService>();
        advisor.ListRecommendationsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string?>(),
            Arg.Any<RecommendationFilters?>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AdvisorQueryPage<Models.Recommendation>([], false));
        var summary = Substitute.For<IRecommendationSummaryService>();
        summary.SummarizeRecommendationsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<RecommendationFilters?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationSummary("category", 0, []));
        var prevalidation = Substitute.For<IPreValidationService>();
        prevalidation.ValidateAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<Models.Recommendation>>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(PreValidationService.BuildResult(TypeId, [], DateTimeOffset.UtcNow, 30, false, []));

        return operation switch
        {
            "list" => new RecommendationListCommand(NullLogger<RecommendationListCommand>.Instance, advisor, resolver),
            "summary" => new RecommendationSummaryCommand(NullLogger<RecommendationSummaryCommand>.Instance, summary, resolver),
            "prevalidate" => new RecommendationPreValidationCommand(NullLogger<RecommendationPreValidationCommand>.Instance, advisor, prevalidation, resolver),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
    }
}
