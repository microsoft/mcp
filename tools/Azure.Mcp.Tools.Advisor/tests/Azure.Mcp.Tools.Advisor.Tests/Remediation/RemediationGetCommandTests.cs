// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Remediation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Remediation;

public class RemediationGetCommandTests : CommandUnitTestsBase<RemediationGetCommand, IRemediationService>
{
    private const string RecommendationTypeId = "18745007-438b-4c68-bfa3-b6576d85a831";

    private static RemediationPackage CreateSamplePackage() => new()
    {
        Id = $"/providers/Microsoft.Advisor/remediations/{RecommendationTypeId}",
        Name = RecommendationTypeId,
        Type = "Microsoft.Advisor/remediations",
        Properties = new RemediationProperties
        {
            RecommendationTypeId = RecommendationTypeId,
            OutputType = "executable",
            Destructive = false,
            Reversible = true,
            Grounded = true,
            Confidence = "medium",
            Version = 1,
            Artifacts =
            [
                new RemediationArtifact
                {
                    ArtifactType = "cli",
                    ContentType = "text/x-shellscript",
                    Confidence = "high",
                    Content = "az webapp config set --name <app-name> --resource-group <resource-group>",
                },
            ],
            Methods =
            [
                new RemediationMethod
                {
                    Heading = "Azure CLI",
                    Method = "cli",
                    Relation = "alternative",
                    Executable = true,
                    Parameters =
                    [
                        new RemediationParameter { Name = "app-name", Description = "The App Service name.", Example = "my-web-app", Required = true },
                    ],
                    Steps =
                    [
                        new RemediationStep { Number = 1, Text = "Apply the remediation command.", Kind = "command", Command = "az webapp config set" },
                    ],
                    Verification = "az webapp config show --name <app-name> --resource-group <resource-group>",
                },
            ],
        },
    };

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--recommendation-type-id 18745007-438b-4c68-bfa3-b6576d85a831", true)]
    [InlineData("--recommendation-type-id not-a-guid", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRemediationPackage()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RemediationGetResult);

        Assert.NotNull(result.Remediation);
        Assert.Equal(RecommendationTypeId, result.Remediation.Name);
        Assert.Equal("Microsoft.Advisor/remediations", result.Remediation.Type);
        Assert.NotNull(result.Remediation.Properties);
        Assert.Equal("executable", result.Remediation.Properties!.OutputType);
        Assert.NotNull(result.Remediation.Properties.Destructive);
        Assert.False(result.Remediation.Properties.Destructive!.Value);

        var artifact = Assert.Single(result.Remediation.Properties.Artifacts!);
        Assert.Equal("cli", artifact.ArtifactType);

        var method = Assert.Single(result.Remediation.Properties.Methods!);
        Assert.Equal("Azure CLI", method.Heading);
        var step = Assert.Single(method.Steps!);
        Assert.Equal(1, step.Number);
    }

    [Fact]
    public async Task ExecuteAsync_PassesRecommendationTypeIdToService()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        await Service.Received(1).GetRemediationAsync(
            RecommendationTypeId,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("18745007-438b-4c68-bfa3-b6576d85a831x")]
    [InlineData("18745007-438b-4c68-bfa3-b6576d85a831z")]
    public async Task ExecuteAsync_InvalidRecommendationId_ReturnsBadRequest(string recommendationId)
    {
        var response = await ExecuteCommandAsync("--recommendation-type-id", recommendationId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("36-character GUID", response.Message);

        await Service.DidNotReceive().GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingRecommendationId_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("No remediation was found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles401Unauthorized()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Service unavailable or network connectivity issues", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
