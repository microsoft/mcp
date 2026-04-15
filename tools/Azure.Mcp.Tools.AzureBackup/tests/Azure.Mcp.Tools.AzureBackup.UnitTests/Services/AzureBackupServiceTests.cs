// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services;

public class AzureBackupServiceTests
{
    private readonly IRsvBackupOperations _rsvOps;
    private readonly IDppBackupOperations _dppOps;
    private readonly ITenantService _tenantService;
    private readonly ILogger<AzureBackupService> _logger;
    private readonly AzureBackupService _service;

    public AzureBackupServiceTests()
    {
        _rsvOps = Substitute.For<IRsvBackupOperations>();
        _dppOps = Substitute.For<IDppBackupOperations>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<AzureBackupService>>();
        _service = new AzureBackupService(_rsvOps, _dppOps, _tenantService, _logger);
    }

    #region ResolveVaultType - Auto-detection fallback

    [Fact]
    public async Task GetVaultAsync_RsvNotFound_FallsThroughToDpp()
    {
        // RSV returns 404 → should try DPP
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
        await _rsvOps.Received(1).GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>());
        await _dppOps.Received(1).GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_RsvForbidden_FallsThroughToDpp()
    {
        // RSV returns 403 → should try DPP (not propagate immediately)
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        _dppOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
    }

    [Fact]
    public async Task GetVaultAsync_RsvUnauthorized_FallsThroughToDpp()
    {
        // RSV returns 401 → should try DPP
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(401, "Unauthorized"));
        _dppOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
    }

    [Fact]
    public async Task GetVaultAsync_BothStacksFail_ThrowsKeyNotFoundException()
    {
        // Both RSV and DPP return 404
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetVaultAsync("myVault", "rg", "sub", null, null, null, CancellationToken.None));

        Assert.Contains("myVault", ex.Message);
        Assert.Contains("--vault-type", ex.Message);
    }

    [Fact]
    public async Task GetVaultAsync_RsvSucceeds_DoesNotCallDpp()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", null, null, null, CancellationToken.None);

        Assert.Equal("RSV", result.VaultType);
        await _dppOps.DidNotReceive().GetVaultAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_ExplicitRsvVaultType_CallsOnlyRsv()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", "rsv", null, null, CancellationToken.None);

        Assert.Equal("RSV", result.VaultType);
        await _dppOps.DidNotReceive().GetVaultAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_ExplicitDppVaultType_CallsOnlyDpp()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _dppOps.GetVaultAsync("myVault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "sub", "dpp", null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
        await _rsvOps.DidNotReceive().GetVaultAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ListVaults - Partial failure

    [Fact]
    public async Task ListVaultsAsync_BothSucceed_ReturnsMergedResults()
    {
        var rsvVaults = new List<BackupVaultInfo>
        {
            new(null, "rsvVault1", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null)
        };
        var dppVaults = new List<BackupVaultInfo>
        {
            new(null, "dppVault1", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null)
        };
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("sub", null, null, null, null, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.Name == "rsvVault1");
        Assert.Contains(result, v => v.Name == "dppVault1");
    }

    [Fact]
    public async Task ListVaultsAsync_RsvFails_ReturnsDppOnly()
    {
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        var dppVaults = new List<BackupVaultInfo>
        {
            new(null, "dppVault1", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null)
        };
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("sub", null, null, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("dppVault1", result[0].Name);
    }

    [Fact]
    public async Task ListVaultsAsync_DppFails_ReturnsRsvOnly()
    {
        var rsvVaults = new List<BackupVaultInfo>
        {
            new(null, "rsvVault1", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null)
        };
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(500, "Internal error"));

        var result = await _service.ListVaultsAsync("sub", null, null, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("rsvVault1", result[0].Name);
    }

    [Fact]
    public async Task ListVaultsAsync_BothFail_ThrowsAggregateException()
    {
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(500, "RSV error"));
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(500, "DPP error"));

        await Assert.ThrowsAsync<AggregateException>(() =>
            _service.ListVaultsAsync("sub", null, null, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task ListVaultsAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _service.ListVaultsAsync("sub", null, null, null, null, cts.Token));
    }

    #endregion

    #region ListProtectableItems - vault-type validation

    [Fact]
    public async Task ListProtectableItemsAsync_DppVaultType_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListProtectableItemsAsync("vault", "rg", "sub", null, null, "dpp", null, null, CancellationToken.None));

        Assert.Contains("RSV", ex.Message);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_RsvVaultType_DelegatesToRsv()
    {
        var expected = new List<ProtectableItemInfo> { new("item1", "SQL", null, null, null, null, null, null, null) };
        _rsvOps.ListProtectableItemsAsync("vault", "rg", "sub", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.ListProtectableItemsAsync("vault", "rg", "sub", null, null, "rsv", null, null, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_NoVaultType_DelegatesToRsv()
    {
        // When no vault type specified, service auto-detects by probing RSV first
        _rsvOps.GetVaultAsync("vault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null));

        var expected = new List<ProtectableItemInfo> { new("item1", "SQL", null, null, null, null, null, null, null) };
        _rsvOps.ListProtectableItemsAsync("vault", "rg", "sub", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.ListProtectableItemsAsync("vault", "rg", "sub", null, null, null, null, null, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_NoVaultType_DppVault_ThrowsArgumentException()
    {
        // When no vault type specified and vault is DPP, service should throw helpful error
        _rsvOps.GetVaultAsync("vault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("vault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListProtectableItemsAsync("vault", "rg", "sub", null, null, null, null, null, CancellationToken.None));

        Assert.Contains("DPP", ex.Message);
        Assert.Contains("Protectable item discovery is only supported", ex.Message);
    }

    #endregion

    #region CreatePolicy - Unsupported option validation

    [Fact]
    public async Task CreatePolicyAsync_UnsupportedOptions_ThrowsNotSupportedException()
    {
        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            _service.CreatePolicyAsync(
                "v", "rg", "sub", "p", "VM", null,
                "Hourly", null, null, "4", "6", "1",
                null, null, CancellationToken.None));

        Assert.Contains("--schedule-frequency", ex.Message);
        Assert.Contains("--weekly-retention-weeks", ex.Message);
        Assert.Contains("--monthly-retention-months", ex.Message);
        Assert.Contains("--yearly-retention-years", ex.Message);
        Assert.Contains("not yet supported", ex.Message);
    }

    [Fact]
    public async Task CreatePolicyAsync_NoUnsupportedOptions_Succeeds()
    {
        var baseResult = new OperationResult("Succeeded", null, "Policy 'p' created in vault 'v'.");
        _rsvOps.GetVaultAsync("v", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "v", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null));
        _rsvOps.CreatePolicyAsync(
            "v", "rg", "sub", "p", "VM",
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(baseResult);

        var result = await _service.CreatePolicyAsync(
            "v", "rg", "sub", "p", "VM", null,
            null, null, "30", null, null, null,
            null, null, CancellationToken.None);

        Assert.Equal("Succeeded", result.Status);
    }

    [Theory]
    [InlineData("Weekly", null, null, null)]
    [InlineData(null, "4", null, null)]
    [InlineData(null, null, "6", null)]
    [InlineData(null, null, null, "1")]
    public async Task CreatePolicyAsync_SingleUnsupportedOption_ThrowsNotSupportedException(
        string? scheduleFrequency, string? weeklyRetention, string? monthlyRetention, string? yearlyRetention)
    {
        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            _service.CreatePolicyAsync(
                "v", "rg", "sub", "p", "VM", null,
                scheduleFrequency, null, null, weeklyRetention, monthlyRetention, yearlyRetention,
                null, null, CancellationToken.None));

        Assert.Contains("not yet supported", ex.Message);
    }

    #endregion

    #region VaultTypeResolver edge cases

    [Fact]
    public void VaultTypeResolver_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VaultTypeResolver.ValidateVaultType("invalid"));
        Assert.Throws<ArgumentException>(() => VaultTypeResolver.ValidateVaultType(""));
        Assert.Throws<ArgumentException>(() => VaultTypeResolver.ValidateVaultType(null));
    }

    [Fact]
    public void VaultTypeResolver_IsVaultTypeSpecified_InvalidType_Throws()
    {
        Assert.Throws<ArgumentException>(() => VaultTypeResolver.IsVaultTypeSpecified("invalid"));
    }

    [Theory]
    [InlineData("rsv", true)]
    [InlineData("RSV", true)]
    [InlineData("dpp", false)]
    public void VaultTypeResolver_IsRsv_ReturnsExpected(string vaultType, bool expected)
    {
        Assert.Equal(expected, VaultTypeResolver.IsRsv(vaultType));
    }

    [Theory]
    [InlineData("dpp", true)]
    [InlineData("DPP", true)]
    [InlineData("rsv", false)]
    public void VaultTypeResolver_IsDpp_ReturnsExpected(string vaultType, bool expected)
    {
        Assert.Equal(expected, VaultTypeResolver.IsDpp(vaultType));
    }

    #endregion

    #region ConfigureImmutability - State normalization

    [Theory]
    [InlineData("Enabled", "Unlocked")]
    [InlineData("enabled", "Unlocked")]
    [InlineData("ENABLED", "Unlocked")]
    [InlineData("Unlocked", "Unlocked")]
    [InlineData("Disabled", "Disabled")]
    [InlineData("Locked", "Locked")]
    public async Task ConfigureImmutabilityAsync_NormalizesState(string inputState, string expectedNormalized)
    {
        // RSV vault probe succeeds
        _rsvOps.GetVaultAsync("vault", "rg", "sub", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "RSV", null, "rg", null, null, null, null, null, null, null, null, null));
        _rsvOps.ConfigureImmutabilityAsync("vault", "rg", "sub", expectedNormalized, null, null, Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Done"));

        var result = await _service.ConfigureImmutabilityAsync("vault", "rg", "sub", inputState, null, null, null, CancellationToken.None);

        Assert.Equal("Succeeded", result.Status);
        await _rsvOps.Received(1).ConfigureImmutabilityAsync("vault", "rg", "sub", expectedNormalized, null, null, Arg.Any<CancellationToken>());
    }

    #endregion

    #region ListVaults - Resource group filtering

    [Fact]
    public async Task ListVaultsAsync_WithResourceGroup_FiltersResults()
    {
        var rsvVaults = new List<BackupVaultInfo>
        {
            new(null, "vault1", "RSV", "eastus", "rg1", null, null, null, null, null, null, null, null, null),
            new(null, "vault2", "RSV", "eastus", "rg2", null, null, null, null, null, null, null, null, null)
        };
        var dppVaults = new List<BackupVaultInfo>
        {
            new(null, "vault3", "DPP", "eastus", "rg1", null, null, null, null, null, null, null, null, null)
        };
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("sub", "rg1", null, null, null, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, v => Assert.Equal("rg1", v.ResourceGroup, ignoreCase: true));
    }

    [Fact]
    public async Task ListVaultsAsync_WithResourceGroup_CaseInsensitive()
    {
        var rsvVaults = new List<BackupVaultInfo>
        {
            new(null, "vault1", "RSV", "eastus", "MyRG", null, null, null, null, null, null, null, null, null)
        };
        _rsvOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("sub", null, null, Arg.Any<CancellationToken>()).Returns(new List<BackupVaultInfo>());

        var result = await _service.ListVaultsAsync("sub", "myrg", null, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("vault1", result[0].Name);
    }

    #endregion
}
