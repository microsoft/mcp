// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Net.Http;
using Azure.Core;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Extension.Tests;

public sealed class CliGenerateCommandTests : CommandUnitTestsBase<CliGenerateCommand, ICliGenerateService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--intent mock_intent", false)]
    [InlineData("--cli-type az", false)]
    [InlineData("--cli-type wrong_cli_type", false)]
    [InlineData("--intent mock_intent --cli-type az", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Command")
                });
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal([shouldSucceed ? 200 : 400], [(int)response.Status]);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Command")
            });

        // Act
        var response = await ExecuteCommandAsync("--intent", "mock_intent", "--cli-type", "az");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ExtensionJsonContext.Default.CliGenerateResult);

        Assert.Equal("az", result.CliType);
        Assert.Equal("Command", result.Command);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GenerateAzureCLICommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--intent", "mock_intent", "--cli-type", "az");

        // Assert
        Assert.Equal([500], [(int)response.Status]);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_SendsClientTypeHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var handler = new HttpClientHandler();
        var mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();
        mockHttpMessageHandler
            .When(x => x.SendPublic(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                capturedRequest = callInfo.Arg<HttpRequestMessage>();
            });
        mockHttpMessageHandler
            .SendPublic(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("az storage account list") });

        var httpClient = new HttpClient(mockHttpMessageHandler);
        var httpClientFactory = new HttpClientFactoryStub(httpClient);

        var tokenCredential = Substitute.For<TokenCredential>();
        tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("mock-token", DateTimeOffset.UtcNow.AddHours(1)));

        var tokenCredentialProvider = Substitute.For<IAzureTokenCredentialProvider>();
        tokenCredentialProvider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(tokenCredential);

        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);

        var service = new CliGenerateService(httpClientFactory, tokenCredentialProvider, cloudConfiguration);

        // Act
        await service.GenerateAzureCLICommandAsync("list my storage accounts", CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("clientType"), "Request should contain 'clientType' header");
        var clientTypeValues = capturedRequest.Headers.GetValues("clientType").ToList();
        Assert.Single(clientTypeValues);
        Assert.Equal("azuremcp", clientTypeValues[0]);
    }

    /// <summary>
    /// Helper class to expose protected Send method for testing.
    /// </summary>
    public abstract class MockHttpMessageHandler : HttpMessageHandler
    {
        public abstract Task<HttpResponseMessage> SendPublic(HttpRequestMessage request, CancellationToken cancellationToken);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => SendPublic(request, cancellationToken);
    }

    /// <summary>
    /// Stub implementation of IHttpClientFactory that returns a fixed HttpClient.
    /// </summary>
    public sealed class HttpClientFactoryStub(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;

        public HttpClient CreateClient() => httpClient;
    }
}
