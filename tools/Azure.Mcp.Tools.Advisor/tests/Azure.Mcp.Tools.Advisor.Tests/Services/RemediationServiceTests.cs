// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class RemediationServiceTests
{
    private const string RecommendationTypeId = "18745007-438b-4c68-bfa3-b6576d85a831";
    private const string FakeToken = "fake-arm-token";

    [Fact]
    public async Task GetRemediationAsync_BuildsExpectedRequestUri()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, MinimalPackageJson));
        var service = CreateService(handler);

        await service.GetRemediationAsync(RecommendationTypeId, TestContext.Current.CancellationToken);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal(
            $"https://management.azure.com/providers/Microsoft.Advisor/remediationTypes/{RecommendationTypeId}?api-version=2025-01-01-preview",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public void BuildRemediationUrl_IncludesApiVersionAndEscapesId()
    {
        var url = RemediationService.BuildRemediationUrl("https://management.azure.com", "a b/c");

        Assert.Equal(
            "https://management.azure.com/providers/Microsoft.Advisor/remediationTypes/a%20b%2Fc?api-version=2025-01-01-preview",
            url);
    }

    [Fact]
    public async Task GetRemediationAsync_AttachesBearerToken()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, MinimalPackageJson));
        var service = CreateService(handler);

        await service.GetRemediationAsync(RecommendationTypeId, TestContext.Current.CancellationToken);

        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues("Authorization", out var authValues));
        Assert.Equal($"Bearer {FakeToken}", Assert.Single(authValues!));
    }

    [Fact]
    public async Task GetRemediationAsync_DeserializesSuccessResponse()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, FullPackageJson));
        var service = CreateService(handler);

        var package = await service.GetRemediationAsync(RecommendationTypeId, TestContext.Current.CancellationToken);

        Assert.Equal(RecommendationTypeId, package.Name);
        Assert.Equal("Microsoft.Advisor/remediationTypes", package.Type);
        Assert.NotNull(package.Properties);
        Assert.Equal("executable", package.Properties!.OutputType);
        Assert.NotNull(package.Properties.Destructive);
        Assert.False(package.Properties.Destructive!.Value);

        var artifact = Assert.Single(package.Properties.Artifacts!);
        Assert.Equal("cli", artifact.ArtifactType);

        var method = Assert.Single(package.Properties.Methods!);
        Assert.Equal("Azure CLI", method.Heading);
        var step = Assert.Single(method.Steps!);
        Assert.Equal(1, step.Number);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task GetRemediationAsync_ErrorStatus_ThrowsHttpRequestException(HttpStatusCode status)
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(status, "{}"));
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetRemediationAsync(RecommendationTypeId, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.StatusCode);
    }

    private static RemediationService CreateService(HttpMessageHandler handler)
    {
        var azureService = Substitute.For<IAzureService>();

        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);

        azureService.GetClient(Arg.Any<string?>()).Returns(new HttpClient(handler));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FakeTokenCredential(FakeToken));

        return new RemediationService(azureService);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json) =>
        new(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private const string MinimalPackageJson = """
        {
          "id": "/providers/Microsoft.Advisor/remediationTypes/18745007-438b-4c68-bfa3-b6576d85a831",
          "name": "18745007-438b-4c68-bfa3-b6576d85a831",
          "type": "Microsoft.Advisor/remediationTypes",
          "properties": { "recommendationTypeId": "18745007-438b-4c68-bfa3-b6576d85a831" }
        }
        """;

    private const string FullPackageJson = """
        {
          "id": "/providers/Microsoft.Advisor/remediationTypes/18745007-438b-4c68-bfa3-b6576d85a831",
          "name": "18745007-438b-4c68-bfa3-b6576d85a831",
          "type": "Microsoft.Advisor/remediationTypes",
          "properties": {
            "recommendationTypeId": "18745007-438b-4c68-bfa3-b6576d85a831",
            "outputType": "executable",
            "destructive": false,
            "reversible": true,
            "grounded": true,
            "confidence": "medium",
            "version": 1,
            "artifacts": [
              {
                "artifactType": "cli",
                "contentType": "text/x-shellscript",
                "confidence": "high",
                "content": "az webapp config set --name <app-name> --resource-group <resource-group>"
              }
            ],
            "methods": [
              {
                "heading": "Azure CLI",
                "method": "cli",
                "relation": "alternative",
                "executable": true,
                "parameters": [
                  { "name": "app-name", "description": "The App Service name.", "example": "my-web-app", "required": true }
                ],
                "steps": [
                  { "number": 1, "text": "Apply the remediation command.", "kind": "command", "command": "az webapp config set" }
                ],
                "verification": "az webapp config show --name <app-name> --resource-group <resource-group>"
              }
            ]
          }
        }
        """;

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(responder(request));
        }
    }

    private sealed class FakeTokenCredential(string token) : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            new(token, DateTimeOffset.UtcNow.AddHours(1));

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            new(GetToken(requestContext, cancellationToken));
    }
}
