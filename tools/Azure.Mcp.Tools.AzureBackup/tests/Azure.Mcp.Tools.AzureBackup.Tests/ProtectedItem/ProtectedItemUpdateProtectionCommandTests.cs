// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.ProtectedItem;

public class ProtectedItemUpdateProtectionCommandTests : SubscriptionCommandUnitTestsBase<ProtectedItemUpdateProtectionCommand, IAzureBackupService>
{
    private const string VmId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update-protection", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ChangePolicyOnly_Succeeds()
    {
        // Arrange
        Service.UpdateProtectionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(VmId),
            Arg.Is<string?>("NewPolicy"),
            Arg.Is<DiskExclusionSpec?>(s => s == null),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("InProgress", "vm1-backup", "job123", "ConfigureBackup started"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--policy", "NewPolicy");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemUpdateProtectionCommandResult);
        Assert.Equal("InProgress", result.Result.Status);
        Assert.Equal("job123", result.Result.JobId);
    }

    [Fact]
    public async Task ExecuteAsync_ChangeDiskExclusionOnly_PassesSpec()
    {
        // Arrange
        DiskExclusionSpec? capturedSpec = null;
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<DiskExclusionSpec?>(s => capturedSpec = s),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("InProgress", "vm1-backup", "job123", null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--disk-list-setting", "exclude",
            "--disks-list", "0,2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedSpec);
        Assert.Equal("exclude", capturedSpec!.Setting);
        Assert.Equal("0,2", capturedSpec.DiskLunsCsv);
    }

    [Fact]
    public async Task ExecuteAsync_ChangePolicyAndDisk_PassesBoth()
    {
        // Arrange
        string? capturedPolicy = null;
        DiskExclusionSpec? capturedSpec = null;
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Do<string?>(p => capturedPolicy = p),
            Arg.Do<DiskExclusionSpec?>(s => capturedSpec = s),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("InProgress", "vm1-backup", "job123", null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--policy", "NewPolicy",
            "--disk-list-setting", "include",
            "--disks-list", "0,1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("NewPolicy", capturedPolicy);
        Assert.NotNull(capturedSpec);
        Assert.Equal("include", capturedSpec!.Setting);
    }

    [Fact]
    public async Task ExecuteAsync_ResetExclusionSettings_Succeeds()
    {
        // Arrange
        DiskExclusionSpec? capturedSpec = null;
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Do<DiskExclusionSpec?>(s => capturedSpec = s),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("InProgress", "vm1-backup", "job123", null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--disk-list-setting", "resetexclusionsettings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedSpec);
        Assert.Equal("resetexclusionsettings", capturedSpec!.Setting);
    }

    [Fact]
    public async Task ExecuteAsync_NoChangesRequested_ReturnsValidationError()
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one of --policy", response.Message);

        await Service.DidNotReceive().UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DiskExclusionSpec?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredArgs_ReturnsValidationError()
    {
        // Act: missing --vault and --resource-group and --datasource-id
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--policy", "NewPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NotSupportedException_MapsToBadRequest()
    {
        // Arrange: simulate DPP-vault rejection from service layer.
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DiskExclusionSpec?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new NotSupportedException("update-protection is only supported for RSV IaaS VM protected items."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--policy", "NewPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("only supported for RSV", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForbiddenFromService_MapsToRbacMessage()
    {
        // Arrange
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DiskExclusionSpec?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "AuthorizationFailed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--policy", "NewPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NotFoundFromService_MapsToFriendlyMessage()
    {
        // Arrange
        Service.UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DiskExclusionSpec?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "NotFound"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
            "--policy", "NewPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not currently protected", response.Message);
    }

    [Theory]
    [InlineData("garbage", null, false, "Invalid --disk-list-setting")]
    [InlineData("include", null, false, "either --disks-list or --exclude-all-data-disks")]
    [InlineData("include", "0,abc", false, "Invalid disk LUN")]
    [InlineData("include", "0,1", true, "mutually exclusive")]
    [InlineData("resetexclusionsettings", "0", false, "cannot be combined")]
    public async Task ExecuteAsync_InvalidDiskOptions_ReturnsValidationError(
        string? setting, string? disksList, bool excludeAll, string expectedFragment)
    {
        // Arrange
        var args = new List<string>
        {
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", VmId,
        };
        if (setting is not null)
        {
            args.Add("--disk-list-setting");
            args.Add(setting);
        }
        if (disksList is not null)
        {
            args.Add("--disks-list");
            args.Add(disksList);
        }
        if (excludeAll)
        {
            args.Add("--exclude-all-data-disks");
        }

        // Act
        var response = await ExecuteCommandAsync([.. args]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedFragment, response.Message);

        await Service.DidNotReceive().UpdateProtectionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<DiskExclusionSpec?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
