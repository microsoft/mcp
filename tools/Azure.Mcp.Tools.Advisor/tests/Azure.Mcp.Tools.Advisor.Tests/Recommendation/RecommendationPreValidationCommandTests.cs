// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public sealed class RecommendationPreValidationCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationPreValidationCommand, IAdvisorService>
{
    private const string TypeId = "48eda464-1485-4dcf-a674-d0905df5054a";
    private const string ResourceId = "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg/providers/Microsoft.Compute/disks/selected";
    private readonly IPreValidationService _validator = Substitute.For<IPreValidationService>();

    [Theory]
    [InlineData("--recommendation-type-id", "invalid")]
    [InlineData("--top", "0")]
    [InlineData("--top", "26")]
    [InlineData("--minimum-unused-days", "0")]
    [InlineData("--minimum-unused-days", "366")]
    [InlineData("--resources", " ")]
    [InlineData("--resources", "/")]
    public async Task ExecuteAsync_RejectsInvalidInputBeforeAzureReads(string option, string value)
    {
        var args = new List<string> { "--subscription", "sub" };
        if (option != "--recommendation-type-id")
        {
            args.AddRange(["--recommendation-type-id", TypeId]);
        }
        args.AddRange([option, value]);

        var response = await InvokeAsync(args.ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
        Assert.Empty(_validator.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ExecuteAsync_ReusesExistingScopeAndAppliesSelectionBeforeLimit(bool serviceGroup, bool multipleSubscriptions)
    {
        RecommendationFilters? capturedFilters = null;
        int capturedTop = 0;
        var candidate = Candidate(ResourceId, "selected");
        Service.ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Do<RecommendationFilters?>(filters => capturedFilters = filters),
            Arg.Do<int>(top => capturedTop = top), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([candidate], false));
        Service.ListRecommendationsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<string?>(),
            Arg.Do<RecommendationFilters?>(filters => capturedFilters = filters),
            Arg.Do<int>(top => capturedTop = top), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([candidate], false));
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<Models.Recommendation>>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(PreValidationService.BuildResult(TypeId, [], DateTimeOffset.UtcNow, 30, false, []));
        var args = new List<string> { "--recommendation-type-id", TypeId, "--resources", ResourceId, "--top", "1" };
        args.AddRange(serviceGroup ? ["--service-group", "commerce"] :
            multipleSubscriptions ? ["--subscriptions", "sub-a", "sub-b"] : ["--subscription", "sub-a"]);

        var response = await InvokeAsync(args.ToArray());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedFilters);
        Assert.Equal("Cost", capturedFilters.Category);
        Assert.Equal("IdleResources", capturedFilters.SubCategory);
        Assert.Equal(RecommendationStatus.New, capturedFilters.Status);
        Assert.Equal(ResourceId, Assert.Single(capturedFilters.Resources!));
        Assert.Equal(serviceGroup ? "commerce" : null, capturedFilters.ServiceGroup);
        Assert.Equal(100, capturedTop);
        await _validator.Received(1).ValidateAsync(TypeId,
            Arg.Is<IReadOnlyList<Models.Recommendation>>(candidates => candidates.Count == 1 && candidates[0] == candidate),
            Arg.Any<string?>(), false, Arg.Is<IReadOnlyList<string>>(missing => missing.Count == 0),
            30, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ReportsUnmatchedSelectionsWithoutValidatingUnrelatedRows()
    {
        Service.ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RecommendationFilters?>(),
            Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([Candidate(ResourceId + "-other", "other")], false));
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<Models.Recommendation>>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(PreValidationService.BuildResult(TypeId, [], DateTimeOffset.UtcNow, 30, false, [ResourceId]));

        var response = await InvokeAsync("--subscription", "sub", "--recommendation-type-id", TypeId, "--resources", ResourceId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.PreValidationResult);
        Assert.Equal(ResourceId, Assert.Single(result.UnmatchedResources));
        await _validator.Received(1).ValidateAsync(TypeId,
            Arg.Is<IReadOnlyList<Models.Recommendation>>(candidates => candidates.Count == 0),
            Arg.Any<string?>(), false, Arg.Is<IReadOnlyList<string>>(missing => missing.Count == 1),
            30, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsAmbiguousNames()
    {
        Service.ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RecommendationFilters?>(),
            Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>(
                [Candidate(ResourceId, "same"), Candidate(ResourceId + "-second", "same")], false));

        var response = await InvokeAsync("--subscription", "sub", "--recommendation-type-id", TypeId, "--resources", "same");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(_validator.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAzureErrorsWithoutReturningBackendDetails()
    {
        Service.ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RecommendationFilters?>(),
            Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "private backend details"));

        var response = await InvokeAsync("--subscription", "sub", "--recommendation-type-id", TypeId);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.DoesNotContain("private backend details", response.Message);
        Assert.Null(response.Results);
        Assert.Empty(_validator.ReceivedCalls());
    }

    [Fact]
    public void Constructor_RegistersTheReadOnlyCommandAndOptions()
    {
        Services.AddSingleton(_validator);
        Assert.Equal("prevalidate", Command.GetCommand().Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Contains("Retail USD", Command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresARecommendationType()
    {
        var response = await InvokeAsync("--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    private Task<CommandResponse> InvokeAsync(params string[] args)
    {
        Services.AddSingleton(_validator);
        return ExecuteCommandAsync(args);
    }

    private static Models.Recommendation Candidate(string id, string name) =>
        new(new RecommendationProperties("Cost", ImpactedValue: name, RecommendationStatus: "New",
            RecommendationTypeId: TypeId, ResourceMetadata: new(id)), Name: "recommendation");
}
