// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Services;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests;

public class SshKeyValidationTests
{
    [Theory]
    [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7... user@host")]
    [InlineData("ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGz... user@host")]
    [InlineData("ssh-dss AAAAB3NzaC1kc3MAAACBAJ... user@host")]
    [InlineData("ecdsa-sha2-nistp256 AAAAE2VjZHNh... user@host")]
    [InlineData("ecdsa-sha2-nistp384 AAAAE2VjZHNh... user@host")]
    [InlineData("ecdsa-sha2-nistp521 AAAAE2VjZHNh... user@host")]
    [InlineData("sk-ssh-ed25519@openssh.com AAAAGnNrLXNz... user@host")]
    [InlineData("sk-ecdsa-sha2-nistp256@openssh.com AAAAInNr... user@host")]
    public void IsValidSshPublicKeyContent_ValidKeys_ReturnsTrue(string key)
    {
        Assert.True(ComputeService.IsValidSshPublicKeyContent(key));
    }

    [Theory]
    [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7...")] // No comment - still valid
    [InlineData("  ssh-rsa AAAAB3NzaC1yc2EAAAA... user@host  ")] // Leading/trailing whitespace
    public void IsValidSshPublicKeyContent_ValidKeysWithVariations_ReturnsTrue(string key)
    {
        Assert.True(ComputeService.IsValidSshPublicKeyContent(key));
    }

    [Theory]
    [InlineData("ssh-rsaNotAKey")] // No space after prefix
    [InlineData("ssh-ed25519NoSpace")] // No space after prefix
    [InlineData("/home/user/.ssh/id_rsa.pub")] // File path
    [InlineData("C:\\Users\\user\\.ssh\\id_rsa.pub")] // Windows file path
    [InlineData("~/.ssh/id_rsa.pub")] // Relative file path
    [InlineData("not-a-key-at-all")] // Random string
    [InlineData("")] // Empty string
    [InlineData("   ")] // Whitespace only
    [InlineData("rsa-sha2-256 AAAA...")] // Wrong prefix format
    [InlineData("SSH-RSA AAAA...")] // Case sensitive - uppercase should fail
    public void IsValidSshPublicKeyContent_InvalidKeys_ReturnsFalse(string key)
    {
        Assert.False(ComputeService.IsValidSshPublicKeyContent(key));
    }

    [Theory]
    [InlineData("/home/user/.ssh/id_rsa.pub")]
    [InlineData("/etc/passwd")]
    [InlineData("/etc/shadow")]
    [InlineData("C:\\Users\\user\\.ssh\\id_rsa.pub")]
    [InlineData("~/.ssh/id_rsa.pub")]
    [InlineData("id_rsa.pub")] // .pub suffix
    [InlineData("my-key.pub")] // .pub suffix
    public void LooksLikeFilePath_FilePaths_ReturnsTrue(string value)
    {
        Assert.True(ComputeService.LooksLikeFilePath(value));
    }

    [Theory]
    [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7...")] // Valid key with / in base64
    [InlineData("not-a-path")]
    [InlineData("just-some-text")]
    public void LooksLikeFilePath_NonPaths_ReturnsFalse(string value)
    {
        Assert.False(ComputeService.LooksLikeFilePath(value));
    }

    [Fact]
    public void LooksLikeFilePath_ValidSshKeyWithSlashInBase64_ReturnsTrueButKeyIsStillValid()
    {
        // This demonstrates why prefix check must run BEFORE file path check:
        // Valid SSH keys contain '/' in base64 content, which would trigger LooksLikeFilePath
        var validKey = "ssh-rsa AAAAB3NzaC1yc2EAAA/DAQAB+AAABgQC7/abc user@host";

        // The key contains '/' so it looks like a file path
        Assert.True(ComputeService.LooksLikeFilePath(validKey));

        // But it's still a valid SSH key by prefix check
        Assert.True(ComputeService.IsValidSshPublicKeyContent(validKey));
    }

    [Fact]
    public void ValidSshKeyPrefixes_ContainsExpectedPrefixes()
    {
        // Verify all standard SSH key types are covered
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ssh-rsa", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ssh-ed25519", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ssh-dss", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ecdsa-sha2-nistp256", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ecdsa-sha2-nistp384", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("ecdsa-sha2-nistp521", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("sk-ssh-ed25519@openssh.com", StringComparison.Ordinal));
        Assert.Contains(ComputeService.s_validSshKeyPrefixes, p => p.StartsWith("sk-ecdsa-sha2-nistp256@openssh.com", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidSshKeyPrefixes_AllEndWithSpace()
    {
        // Each prefix should end with a space to prevent partial matches like "ssh-rsaNotAKey"
        foreach (var prefix in ComputeService.s_validSshKeyPrefixes)
        {
            Assert.True(prefix.EndsWith(' '), $"Prefix '{prefix}' should end with a space");
        }
    }
}
