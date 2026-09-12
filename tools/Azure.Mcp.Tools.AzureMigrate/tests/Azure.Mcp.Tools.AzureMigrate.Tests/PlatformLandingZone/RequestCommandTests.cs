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
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.PlatformLandingZone;

public class RequestCommandTests : SubscriptionCommandUnitTestsBase<RequestCommand, IPlatformLandingZoneService>
{
    private const string Subscription = "sub123";
    private const string ResourceGroup = "rg1";
    private const string ProjectName = "project1";

    public RequestCommandTests()
    {
        Services.AddSingleton(Substitute.For<IAzureService>());
        Services.AddSingleton<AzureMigrateProjectHelper>();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("request", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("create", command.Description);
        Assert.Contains("wait", command.Description);
        Assert.Contains("download", command.Description);
        Assert.Contains("--ddos", command.Description);
    }

    [Theory]
    [InlineData("--action get --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--action list --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--action create --subscription sub123 --resource-group rg1 --migrate-project-name project1 --ddos disabled", true)]
    [InlineData("--action download --subscription sub123 --resource-group rg1 --migrate-project-name project1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --migrate-project-name project1", false)] // Missing action
    [InlineData("--action get --resource-group rg1 --migrate-project-name project1", false)] // Missing subscription
    [InlineData("--action get --subscription sub123 --migrate-project-name project1", false)] // Missing resource group
    [InlineData("--action get --subscription sub123 --resource-group rg1", false)] // Missing migrate project name
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var view = CreateView();

            Service.GetAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
                .Returns(view);

            Service.ListAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
                .Returns([view]);

            Service.CreateOrUpdateAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<Options.PlatformLandingZone.RequestOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(view);

            Service.DownloadAsync(
                Arg.Any<PlatformLandingZoneContext>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .Returns(new[] { "/path/to/output.zip" });
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreateAction_PassesToggleOptionsThrough()
    {
        Service.CreateOrUpdateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<Options.PlatformLandingZone.RequestOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateView());

        var response = await ExecuteCommandAsync(
            "--action", "create",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName,
            "--ddos", "disabled",
            "--bastion", "disabled",
            "--private-dns", "disabled",
            "--express-route", "disabled");

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("was submitted", result.Message);

        await Service.Received(1).CreateOrUpdateAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx =>
                ctx.SubscriptionId == Subscription &&
                ctx.ResourceGroupName == ResourceGroup &&
                ctx.MigrateProjectName == ProjectName),
            Arg.Is<Options.PlatformLandingZone.RequestOptions>(options =>
                options.Ddos == "disabled" &&
                options.Bastion == "disabled" &&
                options.PrivateDns == "disabled" &&
                options.ExpressRoute == "disabled"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysUsesDefaultLandingZoneName()
    {
        Service.GetAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
            .Returns(CreateView());

        await ExecuteCommandAsync(
            "--action", "get",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        await Service.Received(1).GetAsync(
            Arg.Is<PlatformLandingZoneContext>(ctx => ctx.LandingZoneName == "default"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Command_DoesNotExposeALandingZoneNameOption()
    {
        // A migrate project holds exactly one Platform Landing Zone, always named 'default',
        // so the tool must not offer a name parameter.
        var command = Command.GetCommand();

        Assert.DoesNotContain(command.Options, option => option.Name == "landing-zone-name");
    }

    [Fact]
    public async Task ExecuteAsync_GetAction_ReportsMissingLandingZone()
    {
        Service.GetAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
            .Returns((PlatformLandingZoneView?)null);

        var response = await ExecuteCommandAsync(
            "--action", "get",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("No Platform Landing Zone exists", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetAction_ShowsEffectiveConfiguration()
    {
        Service.GetAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
            .Returns(CreateView());

        var response = await ExecuteCommandAsync(
            "--action", "get",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("Generation status: Succeeded", result.Message);
        Assert.Contains("\"deploymentMode\": \"Disabled\"", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ListAction_ReportsEmptyCollection()
    {
        Service.ListAsync(Arg.Any<PlatformLandingZoneContext>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--action", "list",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("No Platform Landing Zone exists", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WaitAction_UsesSuppliedTimeout()
    {
        Service.WaitForTerminalAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateView());

        var response = await ExecuteCommandAsync(
            "--action", "wait",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName,
            "--timeout-minutes", "5");

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("finished generating", result.Message);

        await Service.Received(1).WaitForTerminalAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Is<TimeSpan>(timeout => timeout == TimeSpan.FromMinutes(5)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WaitAction_ReportsFailedGeneration()
    {
        Service.WaitForTerminalAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateView(status: "Failed"));

        var response = await ExecuteCommandAsync(
            "--action", "wait",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("failed to generate", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DownloadAction_ListsDownloadedFiles()
    {
        Service.DownloadAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] { "/path/project1-default-output.zip", "/path/project1-default-design-document.md" });

        var response = await ExecuteCommandAsync(
            "--action", "download",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName,
            "--include-design-document");

        var result = ValidateAndDeserializeResponse(response, AzureMigrateJsonContext.Default.RequestCommandResult);
        Assert.Contains("project1-default-output.zip", result.Message);
        Assert.Contains("project1-default-design-document.md", result.Message);

        await Service.Received(1).DownloadAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAction_ReturnsError()
    {
        var response = await ExecuteCommandAsync(
            "--action", "invalid-action",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        Assert.NotNull(response);
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("Invalid action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.DownloadAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var response = await ExecuteCommandAsync(
            "--action", "download",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesHttpRequestException()
    {
        Service.CreateOrUpdateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<Options.PlatformLandingZone.RequestOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("HTTP request failed"));

        var response = await ExecuteCommandAsync(
            "--action", "create",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        Assert.True(response.Status == HttpStatusCode.BadGateway || response.Status == HttpStatusCode.ServiceUnavailable);
        Assert.Contains("HTTP request failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidOperationException()
    {
        Service.CreateOrUpdateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<Options.PlatformLandingZone.RequestOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("a generation run is still in progress"));

        var response = await ExecuteCommandAsync(
            "--action", "create",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName);

        Assert.True(response.Status == HttpStatusCode.UnprocessableEntity || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("a generation run is still in progress", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        Service.CreateOrUpdateAsync(
            Arg.Any<PlatformLandingZoneContext>(),
            Arg.Any<Options.PlatformLandingZone.RequestOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid ddos value 'maybe'"));

        var response = await ExecuteCommandAsync(
            "--action", "create",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName,
            "--ddos", "maybe");

        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
        Assert.Contains("Invalid ddos value", response.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--action", "create",
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--migrate-project-name", ProjectName,
            "--network-architecture", "hubspoke",
            "--firewall-type", "azurefirewall",
            "--bastion", "disabled",
            "--ddos", "disabled",
            "--private-dns", "disabled",
            "--express-route", "disabled",
            "--vpn-gateway", "disabled",
            "--scale-tier", "full",
            "--regions", "eastus,westus2",
            "--version-control-system", "github",
            "--organization-name", "contoso",
            "--service-name", "payments",
            "--timeout-minutes", "10"
        ]);

        Assert.Empty(args.Errors);
    }

    private static PlatformLandingZoneView CreateView(string name = "default", string status = "Succeeded") =>
        new(
            Name: name,
            ProvisioningState: "Succeeded",
            Status: status,
            ArtifactId: $"/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}/providers/Microsoft.Migrate/migrateProjects/{ProjectName}/artifacts/plz-{name}",
            EffectiveProperties: """
                {
                  "connectivity": {
                    "ddosProtection": {
                      "deploymentMode": "Disabled"
                    }
                  }
                }
                """);
}
