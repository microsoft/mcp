// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.PlatformLandingZone;

public class RequestCommandServiceTests() : SubscriptionCommandUnitTestsBase<RequestCommand, IPlatformLandingZoneService>
{
    private readonly PlatformLandingZoneContext _landingZoneContext = new(
        "00000000-0000-0000-0000-000000000001", "test-rg", $"project-{Guid.NewGuid():N}");
    private readonly List<string> _requests = [];

    [Theory]
    [InlineData(HttpStatusCode.Accepted, "")]
    [InlineData(HttpStatusCode.Accepted, """{"status":"Running"}""")]
    [InlineData(HttpStatusCode.OK, "Generation initiated.")]
    [InlineData(HttpStatusCode.OK, "\"Generation initiated.\"")]
    [InlineData(HttpStatusCode.OK, """{"downloadUrl":null}""")]
    [InlineData(HttpStatusCode.OK, """{"properties":{"downloadUrl":""}}""")]
    public async Task Generate_AcceptedWithoutDownloadUrl_ReportsInProgress(HttpStatusCode status, string body)
    {
        ConfigureService((status, body));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync("generate");
        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.Contains("in progress", result.Message);
        Assert.DoesNotContain("generated successfully", result.Message);
        Assert.DoesNotContain("Download URL:", result.Message);
        Assert.EndsWith("/GeneratePlatformLandingZone", Assert.Single(_requests));
    }

    [Theory]
    [InlineData("""{"downloadUrl":"https://storage.example.com/landing-zone.zip"}""")]
    [InlineData("""{"properties":{"downloadUrl":"https://storage.example.com/landing-zone.zip"}}""")]
    [InlineData("\"https://storage.example.com/landing-zone.zip\"")]
    [InlineData("https://storage.example.com/landing-zone.zip")]
    public async Task Generate_WithDownloadUrl_ReportsCompleted(string body)
    {
        ConfigureService((HttpStatusCode.OK, body));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync("generate");
        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.Contains("generated successfully", result.Message);
        Assert.Contains("Download URL: https://storage.example.com/landing-zone.zip", result.Message);
    }

    [Theory]
    [InlineData("Platform landing zone creation failed: PolicyDenied", "PolicyDenied")]
    [InlineData("\"Platform landing zone CREATION FAILED: PolicyDenied\"", "PolicyDenied")]
    [InlineData("""{"error":{"code":"PolicyDenied","message":"The region is not allowed."}}""", "The region is not allowed.")]
    [InlineData("""{"properties":{"error":{"code":"PolicyDenied","message":"The region is not allowed."}}}""", "The region is not allowed.")]
    [InlineData("""{"status":"Failed","message":"PolicyDenied"}""", "PolicyDenied")]
    [InlineData("""{"properties":{"provisioningState":"Canceled","message":"Operation canceled by user."}}""", "Operation canceled by user.")]
    public async Task Download_AfterGenerationAccepted_ReportsBackendFailure(string body, string expectedDetail)
    {
        ConfigureService((HttpStatusCode.Accepted, ""), (HttpStatusCode.OK, body));
        await UpdateParametersAsync();

        var generation = await ExecuteActionAsync("generate");
        var result = ValidateAndDeserializeResponse(generation, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("in progress", result.Message);

        var download = await ExecuteActionAsync("download");

        Assert.NotEqual(HttpStatusCode.OK, download.Status);
        Assert.Contains(expectedDetail, download.Message);
        Assert.Collection(_requests,
            path => Assert.EndsWith("/GeneratePlatformLandingZone", path),
            path => Assert.EndsWith("/DownloadPlatformLandingZone", path));
    }

    [Theory]
    [InlineData("generate", """{"error":{"code":"PolicyDenied","message":"The region is not allowed."}}""")]
    [InlineData("download", """{"error":{"code":"PolicyDenied","message":"The region is not allowed."}}""")]
    [InlineData("generate", "Platform landing zone creation failed: PolicyDenied. The region is not allowed.")]
    [InlineData("download", "Platform landing zone creation failed: PolicyDenied. The region is not allowed.")]
    public async Task Action_WithBackendFailure_PreservesErrorCodeAndMessage(string action, string body)
    {
        ConfigureService((HttpStatusCode.OK, body));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync(action);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("PolicyDenied", response.Message);
        Assert.Contains("The region is not allowed.", response.Message);
        Assert.Single(_requests);
    }

    [Theory]
    [InlineData("generate", HttpStatusCode.BadRequest)]
    [InlineData("generate", HttpStatusCode.Forbidden)]
    [InlineData("download", HttpStatusCode.InternalServerError)]
    public async Task Action_WithHttpFailure_PreservesBackendDetails(string action, HttpStatusCode status)
    {
        ConfigureService((status, """{"error":{"code":"GenerationError","message":"Unable to create the landing zone."}}"""));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync(action);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains(((int)status).ToString(), response.Message);
        Assert.Contains("GenerationError", response.Message);
        Assert.Contains("Unable to create the landing zone.", response.Message);
        Assert.Single(_requests);
    }

    [Fact]
    public async Task Generate_WithHttpFailure_PreservesHttpStatusCode()
    {
        var service = ConfigureService((HttpStatusCode.Forbidden, "Authorization failed."));
        await UpdateParametersAsync();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GenerateAsync(_landingZoneContext, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Contains("Authorization failed.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("""{"status":"Running"}""")]
    [InlineData("""{"properties":{"downloadUrl":null}}""")]
    public async Task Download_WithoutDownloadUrl_ReportsNotReady(string body)
    {
        ConfigureService((HttpStatusCode.OK, body));

        var response = await ExecuteActionAsync("download");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("Download URL not yet available", response.Message);
        Assert.Single(_requests);
    }

    [Theory]
    [InlineData("""{"downloadUrl":"Generation initiated."}""")]
    [InlineData("""{"downloadUrl":"file:///landing-zone.zip"}""")]
    [InlineData("""{"properties":{"downloadUrl":42}}""")]
    [InlineData("""{"downloadUrl":{}}""")]
    public async Task Generate_WithInvalidDownloadUrl_DoesNotReportSuccess(string body)
    {
        ConfigureService((HttpStatusCode.OK, body));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync("generate");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("invalid download URL", response.Message);
    }

    [Fact]
    public async Task Generate_WithMalformedJson_DoesNotReportInProgress()
    {
        ConfigureService((HttpStatusCode.OK, """{"downloadUrl":"""));
        await UpdateParametersAsync();

        var response = await ExecuteActionAsync("generate");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task Check_WithNotFoundResponse_StillReportsNoLandingZone()
    {
        ConfigureService((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Not found.") });
        });

        var response = await ExecuteActionAsync("check");
        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);

        Assert.Contains("No Platform Landing zone found", result.Message);
    }

    [Theory]
    [InlineData("""{"downloadUrl":"https://storage.example.com/landing-zone.zip"}""")]
    [InlineData("""{"properties":{"downloadUrl":"https://storage.example.com/landing-zone.zip"}}""")]
    [InlineData("\"https://storage.example.com/landing-zone.zip\"")]
    [InlineData("https://storage.example.com/landing-zone.zip")]
    public async Task Download_AfterGenerationAccepted_WritesReturnedBytesWhenReady(string body)
    {
        byte[] expectedBytes = [0x50, 0x4b, 0x03, 0x04];
        var service = ConfigureService((request, _) =>
        {
            _requests.Add(request.RequestUri!.AbsolutePath);
            if (_requests.Count == 1)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/GeneratePlatformLandingZone", request.RequestUri.AbsolutePath);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
            }

            if (_requests.Count == 2)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/DownloadPlatformLandingZone", request.RequestUri.AbsolutePath);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
            }

            Assert.Equal(3, _requests.Count);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://storage.example.com/landing-zone.zip", request.RequestUri.AbsoluteUri);
            Assert.Null(request.Headers.Authorization);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(expectedBytes) });
        });
        var directory = Directory.CreateTempSubdirectory("azuremigrate-tests-");
        try
        {
            await UpdateParametersAsync();
            var generation = await ExecuteActionAsync("generate");
            var result = ValidateAndDeserializeResponse(generation, AzureMigrateJsonContext.Default.RequestCommandResult);
            Assert.Contains("in progress", result.Message);

            var path = await service.DownloadAsync(_landingZoneContext, directory.FullName, TestContext.Current.CancellationToken);

            Assert.Equal(directory.FullName, Path.GetDirectoryName(path));
            Assert.EndsWith(".zip", path);
            Assert.Equal(expectedBytes, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
            Assert.Equal(3, _requests.Count);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private PlatformLandingZoneService ConfigureService(params (HttpStatusCode Status, string Body)[] responses)
    {
        var queue = new Queue<(HttpStatusCode Status, string Body)>(responses);
        return ConfigureService((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            _requests.Add(request.RequestUri!.AbsolutePath);
            Assert.NotEmpty(queue);
            var (status, body) = queue.Dequeue();
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        });
    }

    private PlatformLandingZoneService ConfigureService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var azureService = Substitute.For<IAzureService>();
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloud);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new PlaybackTokenCredential());
        azureService.GetClient().Returns(_ => new HttpClient(new AzureMigrateTestHttpMessageHandler(sendAsync)));
        var service = new PlatformLandingZoneService(azureService, new AzureHttpHelper(azureService),
            NullLogger<PlatformLandingZoneService>.Instance);
        Services.AddSingleton(azureService);
        Services.AddSingleton<AzureMigrateProjectHelper>();
        Services.AddSingleton<IPlatformLandingZoneService>(service);
        return service;
    }

    private Task<PlatformLandingZoneParameters> UpdateParametersAsync()
        => ServiceProvider.GetRequiredService<IPlatformLandingZoneService>().UpdateParametersAsync(
            _landingZoneContext, cancellationToken: TestContext.Current.CancellationToken);

    private Task<CommandResponse> ExecuteActionAsync(string action)
        => ((IBaseCommand)Command).ExecuteAsync(new CommandContext(), CommandDefinition.Parse(
            ["--action", action, "--subscription", _landingZoneContext.SubscriptionId,
             "--resource-group", _landingZoneContext.ResourceGroupName,
             "--migrate-project-name", _landingZoneContext.MigrateProjectName]),
            TestContext.Current.CancellationToken);
}
