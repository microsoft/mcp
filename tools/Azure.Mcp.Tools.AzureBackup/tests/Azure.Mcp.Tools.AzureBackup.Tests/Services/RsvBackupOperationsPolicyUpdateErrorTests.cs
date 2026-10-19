// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Unit tests for the internal <see cref="RsvBackupOperations.CreatePolicyMaterializationError"/>
/// helper. When updating an RSV protection policy, the RecoveryServicesBackup SDK can throw a
/// <see cref="NullReferenceException"/> from the <c>BackupProtectionPolicyResource</c> constructor
/// while materializing the CreateOrUpdate response for some IaaS VM policy shapes. The service
/// translates that opaque failure into an actionable <see cref="RequestFailedException"/>; these
/// tests pin the status code and the user-facing message contract.
/// </summary>
public class RsvBackupOperationsPolicyUpdateErrorTests
{
    [Fact]
    public void CreatePolicyMaterializationError_UsesInternalServerErrorStatus()
    {
        var ex = RsvBackupOperations.CreatePolicyMaterializationError("daily-policy", "my-vault");

        Assert.Equal((int)HttpStatusCode.InternalServerError, ex.Status);
    }

    [Fact]
    public void CreatePolicyMaterializationError_MessageIncludesPolicyAndVaultAndVerificationStep()
    {
        var ex = RsvBackupOperations.CreatePolicyMaterializationError("daily-policy", "my-vault");

        Assert.Contains("daily-policy", ex.Message);
        Assert.Contains("my-vault", ex.Message);
        // Directs the caller to confirm whether the submitted change applied.
        Assert.Contains("policy get", ex.Message);
    }
}
