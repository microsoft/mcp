// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.KeyVault.Commands;
using Azure.Mcp.Tools.KeyVault.Commands.Admin;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Administration;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.Tests.Admin;

public class AdminSettingsGetCommandTests : CommandUnitTestsBase<AdminSettingsGetCommand, IKeyVaultService>
{
    private const string KnownVaultName = "knownVaultName";

    [Fact]
    public async Task ExecuteAsync_ReturnsSettingsDictionary()
    {
        // We return null from service (simplest stub); command should still succeed with empty dictionary.
        Service.GetVaultSettings(
            Arg.Is(KnownVaultName),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((GetSettingsResult)null!);

        var response = await ExecuteCommandAsync("--vault", KnownVaultName);

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
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--vault", KnownVaultName);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--vault knownVaultName", true)]
    [InlineData("", false, "Missing required vault")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedFailureReason = "")
    {
        if (shouldSucceed)
        {
            // Service returns null result -> treated as empty settings
            Service.GetVaultSettings(
                Arg.Any<string>(),
                Arg.Any<string?>(),
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

    [Fact]
    public void Constructor_DoesNotExposeSubscriptionOption() =>
        Assert.DoesNotContain(Command.GetCommand().Options, option => option.Name == "--subscription");
}
