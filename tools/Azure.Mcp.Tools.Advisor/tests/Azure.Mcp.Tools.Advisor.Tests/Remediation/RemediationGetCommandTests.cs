// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Remediation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Remediation;

public class RemediationGetCommandTests : CommandUnitTestsBase<RemediationGetCommand, IRemediationService>
{
    private const string RecommendationId = "18745007-438b-4c68-bfa3-b6576d85a831";

    private static RemediationPackage CreateSamplePackage() => new()
    {
        Id = $"/providers/Microsoft.Advisor/remediationTypes/{RecommendationId}",
        Name = RecommendationId,
        Type = "Microsoft.Advisor/remediationTypes",
        Properties = new RemediationProperties
        {
            RecommendationTypeId = RecommendationId,
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
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831", true)]
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831 --artifact-types cli", true)]
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831 --artifact-types cli,bicep", true)]
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831 --artifact-types cli,powershell,bicep,arm", true)]
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831 --artifact-types yaml", false)]
    [InlineData("--recommendation-id 18745007-438b-4c68-bfa3-b6576d85a831 --artifact-types cli,yaml", false)]
    [InlineData("--recommendation-id not-a-guid", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
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
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        var response = await ExecuteCommandAsync("--recommendation-id", RecommendationId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RemediationGetResult);

        Assert.NotNull(result.Remediation);
        Assert.Equal(RecommendationId, result.Remediation.Name);
        Assert.Equal("Microsoft.Advisor/remediationTypes", result.Remediation.Type);
        Assert.NotNull(result.Remediation.Properties);
        Assert.Equal("executable", result.Remediation.Properties!.OutputType);
        Assert.False(result.Remediation.Properties.Destructive);

        var artifact = Assert.Single(result.Remediation.Properties.Artifacts!);
        Assert.Equal("cli", artifact.ArtifactType);

        var method = Assert.Single(result.Remediation.Properties.Methods!);
        Assert.Equal("Azure CLI", method.Heading);
        var step = Assert.Single(method.Steps!);
        Assert.Equal(1, step.Number);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsArtifactTypesToNull()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        await ExecuteCommandAsync("--recommendation-id", RecommendationId);

        await Service.Received(1).GetRemediationAsync(
            RecommendationId,
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesArtifactTypesToService()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateSamplePackage());

        await ExecuteCommandAsync("--recommendation-id", RecommendationId, "--artifact-types", "cli,bicep");

        await Service.Received(1).GetRemediationAsync(
            RecommendationId,
            Arg.Is<string[]>(a => a.Length == 2 && a[0] == "cli" && a[1] == "bicep"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("18745007-438b-4c68-bfa3-b6576d85a831x")]
    [InlineData("18745007-438b-4c68-bfa3-b6576d85a831z")]
    public async Task ExecuteAsync_InvalidRecommendationId_ReturnsBadRequest(string recommendationId)
    {
        var response = await ExecuteCommandAsync("--recommendation-id", recommendationId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("36-character GUID", response.Message);

        await Service.DidNotReceive().GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingRecommendationId_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--artifact-types", "cli");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("yaml")]
    [InlineData("cli,yaml")]
    [InlineData("terraform")]
    public async Task ExecuteAsync_UnsupportedArtifactType_ReturnsBadRequest(string artifactTypes)
    {
        var response = await ExecuteCommandAsync("--recommendation-id", RecommendationId, "--artifact-types", artifactTypes);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("unsupported value", response.Message, StringComparison.OrdinalIgnoreCase);

        await Service.DidNotReceive().GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

        var response = await ExecuteCommandAsync("--recommendation-id", RecommendationId);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("No remediation was found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles401Unauthorized()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

        var response = await ExecuteCommandAsync("--recommendation-id", RecommendationId);

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Authentication failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--recommendation-id", RecommendationId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
