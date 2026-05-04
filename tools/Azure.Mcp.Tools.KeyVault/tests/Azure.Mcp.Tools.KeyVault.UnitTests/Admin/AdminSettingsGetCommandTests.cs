// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.KeyVault.Commands;
using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Administration;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.UnitTests.Admin;

public class AdminSettingsGetCommandTests : CommandUnitTestsBase<AdminSettingsGetCommand, IKeyVaultService>
{
    private const string KnownSubscriptionId = "knownSubscription";
    private const string KnownVaultName = "knownVaultName";

    [Fact]
    public async Task ExecuteAsync_ReturnsSettingsDictionary()
    {
        // We return null from service (simplest stub); command should still succeed with empty dictionary.
        Service.GetVaultSettings(
            Arg.Is(KnownVaultName),
            Arg.Is(KnownSubscriptionId),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((GetSettingsResult)null!);

        var response = await ExecuteCommandAsync("--vault", KnownVaultName, "--subscription", KnownSubscriptionId);

        var result = ValidateAndDeserializeResponse(response, KeyVaultJsonContext.Default.AdminSettingsGetCommandResult);
        Assert.Equal(KnownVaultName, result.Name);
        Assert.NotNull(result.Settings);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.GetVaultSettings(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--vault", KnownVaultName, "--subscription", KnownSubscriptionId);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--vault knownVaultName --subscription knownSubscription", true)]
    [InlineData("--subscription knownSubscription --vault knownVaultName", true)]
    [InlineData("--vault knownVaultName", true)] // Subscription from env var
    [InlineData("--subscription knownSubscription", false, "Missing required vault")] // Missing required vault
    [InlineData("", false, "Missing both")] // Missing both
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedFailureReason = "")
    {
        if (args.Contains("--vault") && !args.Contains("--subscription") && shouldSucceed)
        {
            // Provide subscription via environment variable
            TestEnvironment.SetAzureSubscriptionId(KnownSubscriptionId);
        }
        else if (!args.Contains("--subscription"))
        {
            // Ensure failure when subscription missing and not expected to succeed
            Assert.Null(EnvironmentHelpers.GetAzureSubscriptionId());
        }

        if (shouldSucceed)
        {
            // Service returns null result -> treated as empty settings
            Service.GetVaultSettings(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns((GetSettingsResult)null!);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"Validation failed as expected: {expectedFailureReason}");
        }
    }
}
