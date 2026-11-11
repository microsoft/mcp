// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class BackupJobIdParserTests
{
    private const string JobGuid = "9f1c2a3b-0000-4000-8000-123456789abc";
    private const string Sub = "00000000-0000-0000-0000-000000000001";
    private const string RsvJobId = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1/backupJobs/{JobGuid}";
    private const string DppJobId = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.DataProtection/backupVaults/vault1/backupJobs/{JobGuid}";

    [Theory]
    [InlineData(JobGuid)]
    [InlineData(" " + JobGuid + " ")]
    [InlineData(JobGuid + "/")]
    [InlineData("/" + JobGuid)]
    [InlineData("backupJobs/" + JobGuid)]
    [InlineData("BackupJobs/" + JobGuid + "/")]
    public void TryParse_AcceptsJobNameForms(string job)
    {
        Assert.True(BackupJobIdParser.TryParse(job, "vault1", "rg1", null, out var reference, out var error));
        Assert.Null(error);
        Assert.Equal(JobGuid, reference.JobName);
        Assert.Null(reference.VaultType);
    }

    [Theory]
    [InlineData(RsvJobId, "rsv")]
    [InlineData(RsvJobId + "/", "rsv")]
    [InlineData(DppJobId, "dpp")]
    public void TryParse_FullArmJobId_ReturnsNameAndInfersVaultType(string job, string expectedVaultType)
    {
        Assert.True(BackupJobIdParser.TryParse(job, "VAULT1", "RG1", null, out var reference, out var error));
        Assert.Null(error);
        Assert.Equal(JobGuid, reference.JobName);
        Assert.Equal(expectedVaultType, reference.VaultType);
    }

    [Fact]
    public void TryParse_FullArmJobId_WithMatchingVaultType_Succeeds()
    {
        Assert.True(BackupJobIdParser.TryParse(DppJobId, "vault1", "rg1", "DPP", out var reference, out _));
        Assert.Equal("dpp", reference.VaultType);
    }

    [Fact]
    public void TryParse_FullArmJobId_WithDifferentVault_Fails()
    {
        Assert.False(BackupJobIdParser.TryParse(RsvJobId, "otherVault", "rg1", null, out _, out var error));
        Assert.Contains("vault1", error);
        Assert.Contains("--vault", error);
    }

    [Fact]
    public void TryParse_FullArmJobId_WithDifferentResourceGroup_Fails()
    {
        Assert.False(BackupJobIdParser.TryParse(RsvJobId, "vault1", "otherRg", null, out _, out var error));
        Assert.Contains("--resource-group", error);
    }

    [Fact]
    public void TryParse_FullArmJobId_WithConflictingVaultType_Fails()
    {
        Assert.False(BackupJobIdParser.TryParse(RsvJobId, "vault1", "rg1", "dpp", out _, out var error));
        Assert.Contains("--vault-type", error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/")]
    [InlineData("a/b/c")]
    [InlineData("vaults/vault1/backupJobs/" + JobGuid)]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1")]
    public void TryParse_RejectsMalformedValues(string? job)
    {
        Assert.False(BackupJobIdParser.TryParse(job, "vault1", "rg1", null, out var reference, out var error));
        Assert.Null(reference);
        Assert.Equal(BackupJobIdParser.InvalidFormatMessage, error);
    }

    [Fact]
    public void TryParse_InvalidVaultType_DoesNotThrow()
    {
        Assert.True(BackupJobIdParser.TryParse(RsvJobId, "vault1", "rg1", "not-a-type", out var reference, out _));
        Assert.Equal("rsv", reference.VaultType);
    }
}
