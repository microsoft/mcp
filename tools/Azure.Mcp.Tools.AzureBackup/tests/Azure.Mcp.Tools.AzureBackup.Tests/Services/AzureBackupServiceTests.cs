// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class AzureBackupServiceTests
{
    private readonly IRsvBackupOperations _rsvOps;
    private readonly IDppBackupOperations _dppOps;
    private readonly ITenantService _tenantService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<AzureBackupService> _logger;
    private readonly AzureBackupService _service;

    public AzureBackupServiceTests()
    {
        _rsvOps = Substitute.For<IRsvBackupOperations>();
        _dppOps = Substitute.For<IDppBackupOperations>();
        _tenantService = Substitute.For<ITenantService>();
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _logger = Substitute.For<ILogger<AzureBackupService>>();
        _service = new AzureBackupService(_rsvOps, _dppOps, _tenantService, _subscriptionService, _logger);
    }

    #region ResolveVaultType - Auto-detection fallback

    [Fact]
    public async Task GetVaultAsync_RsvNotFound_FallsThroughToDpp()
    {
        // RSV returns 404 -> should try DPP
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
        await _rsvOps.Received(1).GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>());
        await _dppOps.Received(1).GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_RsvForbidden_FallsThroughToDpp()
    {
        // RSV returns 403 -> should try DPP (not propagate immediately)
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
    }

    [Fact]
    public async Task GetVaultAsync_RsvUnauthorized_FallsThroughToDpp()
    {
        // RSV returns 401 -> should try DPP
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(401, "Unauthorized"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None);

        Assert.Equal("DPP", result.VaultType);
    }

    [Fact]
    public async Task GetVaultAsync_BothStacksFail_ThrowsKeyNotFoundException()
    {
        // Both RSV and DPP return 404
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None));

        Assert.Contains("myVault", ex.Message);
        Assert.Contains("--vault-type", ex.Message);
    }

    [Fact]
    public async Task GetVaultAsync_BothStacksForbidden_ThrowsUnauthorizedAccessException()
    {
        // Both RSV and DPP return 403 -> should throw UnauthorizedAccessException (not KeyNotFoundException)
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None));

        Assert.Contains("Authorization failed", ex.Message);
        Assert.Contains("RBAC permissions", ex.Message);
    }

    [Fact]
    public async Task GetVaultAsync_BothStacksUnauthorized_ThrowsUnauthorizedAccessException()
    {
        // Both RSV and DPP return 401 -> should throw UnauthorizedAccessException
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(401, "Unauthorized"));
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(401, "Unauthorized"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None));

        Assert.Contains("Authorization failed", ex.Message);
    }

    [Fact]
    public async Task GetVaultAsync_RsvSucceeds_DoesNotCallDpp()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, CancellationToken.None);

        Assert.Equal("RSV", result.VaultType);
        await _dppOps.DidNotReceive().GetVaultAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_ExplicitRsvVaultType_CallsOnlyRsv()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _rsvOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", "rsv", null, null, CancellationToken.None);

        Assert.Equal("RSV", result.VaultType);
        await _dppOps.DidNotReceive().GetVaultAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetVaultAsync_ExplicitDppVaultType_CallsOnlyDpp()
    {
        var expectedVault = new BackupVaultInfo(null, "myVault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null);
        _dppOps.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var result = await _service.GetVaultAsync("myVault", "rg", "22222222-2222-2222-2222-222222222222", "dpp", null, null, CancellationToken.None);

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
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.Name == "rsvVault1");
        Assert.Contains(result, v => v.Name == "dppVault1");
    }

    [Fact]
    public async Task ListVaultsAsync_RsvFails_ReturnsDppOnly()
    {
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));
        var dppVaults = new List<BackupVaultInfo>
        {
            new(null, "dppVault1", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null)
        };
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None);

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
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(500, "Internal error"));

        var result = await _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("rsvVault1", result[0].Name);
    }

    [Fact]
    public async Task ListVaultsAsync_BothFailWithRequestFailedException_ThrowsRequestFailedException()
    {
        // NEW-1: both inner messages are preserved in the combined message.
        // NEW-5: when both inners are RequestFailedException, the wrapper itself is a
        // RequestFailedException so the command-layer error mapper classifies the failure
        // as an Azure service error (with the original HTTP status code) rather than as
        // an MCP-side bug.
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "RSV error"));
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "DPP error"));

        var ex = await Assert.ThrowsAsync<RequestFailedException>(() =>
            _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None));

        Assert.Equal(403, ex.Status);
        Assert.Contains("RSV error", ex.Message);
        Assert.Contains("DPP error", ex.Message);
        Assert.IsType<RequestFailedException>(ex.InnerException);
    }

    [Fact]
    public async Task ListVaultsAsync_BothFailWithRequestFailedException_PrefersNonZeroStatus()
    {
        // NEW-5: when the RSV RequestFailedException carries a 0 status (e.g. transport-level
        // failure with no HTTP status), prefer the DPP status so the user still sees a
        // useful HTTP code.
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(0, "RSV transport error"));
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(503, "DPP throttled"));

        var ex = await Assert.ThrowsAsync<RequestFailedException>(() =>
            _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None));

        Assert.Equal(503, ex.Status);
    }

    [Fact]
    public async Task ListVaultsAsync_BothFailWithRequestFailedException_StatusAndErrorCodePairedFromSameSource()
    {
        // NEW-5: Status and ErrorCode must come from the same exception so callers
        // never see a mismatched (Status, ErrorCode) pair. Here RSV has Status=0 with
        // its own ErrorCode; the wrapper should take both fields from DPP (the source
        // of the non-zero Status).
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(0, "RSV transport error", "RsvOnlyCode", null));
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(503, "DPP throttled", "DppThrottled", null));

        var ex = await Assert.ThrowsAsync<RequestFailedException>(() =>
            _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None));

        Assert.Equal(503, ex.Status);
        Assert.Equal("DppThrottled", ex.ErrorCode);
    }

    [Fact]
    public async Task ListVaultsAsync_BothFailWithDifferentExceptions_ThrowsInvalidOperationWithBothMessages()
    {
        // NEW-1: when the two backend failures are not directly comparable, wrap them
        // in a single InvalidOperationException whose Message mentions both inner
        // exception messages so the caller still has actionable context.
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "RSV forbidden"));
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DPP network issue"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, CancellationToken.None));

        Assert.Contains("RSV forbidden", ex.Message);
        Assert.Contains("DPP network issue", ex.Message);
    }

    [Fact]
    public async Task ListVaultsAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, null, null, cts.Token));
    }

    #endregion

    #region ListProtectableItems - vault-type validation

    [Fact]
    public async Task ListProtectableItemsAsync_DppVaultType_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, "dpp", null, null, CancellationToken.None));

        Assert.Contains("RSV", ex.Message);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_RsvVaultType_DelegatesToRsv()
    {
        var expected = new List<ProtectableItemInfo> { new("item1", "SQL", null, null, null, null, null, null, null) };
        _rsvOps.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, "rsv", null, null, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_NoVaultType_DelegatesToRsv()
    {
        // When no vault type specified, service auto-detects by probing RSV first
        _rsvOps.GetVaultAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null));

        var expected = new List<ProtectableItemInfo> { new("item1", "SQL", null, null, null, null, null, null, null) };
        _rsvOps.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, null, null, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListProtectableItemsAsync_NoVaultType_DppVault_ThrowsArgumentException()
    {
        // When no vault type specified and vault is DPP, service should throw helpful error
        _rsvOps.GetVaultAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));
        _dppOps.GetVaultAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "DPP", "eastus", "rg", null, null, null, null, null, null, null, null, null));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ListProtectableItemsAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, null, null, null, CancellationToken.None));

        Assert.Contains("DPP", ex.Message);
        Assert.Contains("Protectable item discovery is only supported", ex.Message);
    }

    #endregion

    #region CreatePolicy

    [Fact]
    public async Task CreatePolicyAsync_Succeeds()
    {
        var baseResult = new OperationResult("Succeeded", null, "Policy 'p' created in vault 'v'.");
        _rsvOps.GetVaultAsync("v", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "v", "RSV", "eastus", "rg", null, null, null, null, null, null, null, null, null));
        _rsvOps.CreatePolicyAsync(
            Arg.Is<PolicyCreateRequest>(r => r.Policy == "p" && r.WorkloadType == "VM" && r.DailyRetentionDays == "30"),
            "v", "rg", "22222222-2222-2222-2222-222222222222",
            Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(baseResult);

        var request = new PolicyCreateRequest
        {
            Policy = "p",
            WorkloadType = "VM",
            DailyRetentionDays = "30",
        };

        var result = await _service.CreatePolicyAsync(
            request,
            "v", "rg", "22222222-2222-2222-2222-222222222222", null,
            null, null, CancellationToken.None);

        Assert.Equal("Succeeded", result.Status);
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
        _rsvOps.GetVaultAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "RSV", null, "rg", null, null, null, null, null, null, null, null, null));
        _rsvOps.ConfigureImmutabilityAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", expectedNormalized, null, null, Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, "Done"));

        var result = await _service.ConfigureImmutabilityAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", inputState, null, null, null, CancellationToken.None);

        Assert.Equal("Succeeded", result.Status);
        await _rsvOps.Received(1).ConfigureImmutabilityAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", expectedNormalized, null, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Enable")]
    public async Task ConfigureImmutabilityAsync_InvalidState_ThrowsArgumentException(string inputState)
    {
        // RSV vault probe succeeds
        _rsvOps.GetVaultAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>())
            .Returns(new BackupVaultInfo(null, "vault", "RSV", null, "rg", null, null, null, null, null, null, null, null, null));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ConfigureImmutabilityAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", inputState, null, null, null, CancellationToken.None));

        Assert.Contains("Invalid immutability state", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConfigureImmutabilityAsync_EmptyOrWhitespace_ThrowsArgumentException(string inputState)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ConfigureImmutabilityAsync("vault", "rg", "22222222-2222-2222-2222-222222222222", inputState, null, null, null, CancellationToken.None));

        Assert.Contains("immutabilityState", ex.Message);
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
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(dppVaults);

        var result = await _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", "rg1", null, null, null, CancellationToken.None);

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
        _rsvOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(rsvVaults);
        _dppOps.ListVaultsAsync("22222222-2222-2222-2222-222222222222", null, null, Arg.Any<CancellationToken>()).Returns(new List<BackupVaultInfo>());

        var result = await _service.ListVaultsAsync("22222222-2222-2222-2222-222222222222", "myrg", null, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("vault1", result[0].Name);
    }

    #endregion

    #region NEW-3: subscription name -> GUID resolution

    [Fact]
    public async Task ListVaultsAsync_WhenSubscriptionIsName_ResolvesToGuidBeforeCallingOps()
    {
        // NEW-3: --subscription accepts a subscription NAME (not GUID). The service
        // must resolve the name to a GUID before passing it to RSV/DPP ops, which
        // pass it to ResourceIdentifier and otherwise crash with FormatException.
        const string name = "My Production Sub";
        const string resolvedId = "33333333-3333-3333-3333-333333333333";

        var subData = Azure.ResourceManager.Models.ResourceManagerModelFactory.SubscriptionData(
            id: new Azure.Core.ResourceIdentifier($"/subscriptions/{resolvedId}"),
            subscriptionId: resolvedId,
            displayName: name,
            tenantId: null,
            state: Azure.ResourceManager.Resources.Models.SubscriptionState.Enabled);
        var subResource = Substitute.For<Azure.ResourceManager.Resources.SubscriptionResource>();
        subResource.Data.Returns(subData);
        _subscriptionService.GetSubscription(name, null, null, Arg.Any<CancellationToken>()).Returns(subResource);

        _rsvOps.ListVaultsAsync(resolvedId, null, null, Arg.Any<CancellationToken>()).Returns([]);
        _dppOps.ListVaultsAsync(resolvedId, null, null, Arg.Any<CancellationToken>()).Returns([]);

        await _service.ListVaultsAsync(name, null, null, null, null, CancellationToken.None);

        // Ops must have been called with the resolved GUID, NOT the original name.
        await _rsvOps.Received(1).ListVaultsAsync(resolvedId, null, null, Arg.Any<CancellationToken>());
        await _dppOps.Received(1).ListVaultsAsync(resolvedId, null, null, Arg.Any<CancellationToken>());
        await _rsvOps.DidNotReceive().ListVaultsAsync(name, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListVaultsAsync_WhenSubscriptionIsGuid_DoesNotCallSubscriptionService()
    {
        // NEW-3: GUID short-circuit - ResolveSubscriptionIdAsync must NOT call out
        // to ISubscriptionService when the value already parses as a Guid.
        const string guid = "44444444-4444-4444-4444-444444444444";

        _rsvOps.ListVaultsAsync(guid, null, null, Arg.Any<CancellationToken>()).Returns([]);
        _dppOps.ListVaultsAsync(guid, null, null, Arg.Any<CancellationToken>()).Returns([]);

        await _service.ListVaultsAsync(guid, null, null, null, null, CancellationToken.None);

        await _subscriptionService.DidNotReceive().GetSubscription(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
