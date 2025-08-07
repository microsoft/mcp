// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Options;

namespace AzureMcp.Areas.Startups.Services;

public interface IStartupsService
{
    /// <summary>
    /// Gets Microsoft for Startups guidance information.
    /// </summary>
    /// <returns>Guidance information for startups.</returns>
    Task<StartupsGuidanceInfo> GetGuidanceAsync();

    /// <summary>
    /// Deploys code files to Azure storage account.
    /// </summary>
    Task<StartupsDeployResources> DeployStaticWebAsync(
        string tenantId,
        string subscription,
        string storageAccount,
        string resourceGroup,
        string sourcePath,
        RetryPolicyOptions retryPolicy,
        bool overwrite,
        IProgress<string>? progress = null);
    Task<StartupsDeployResources> DeployReactAppAsync(
        string tenantId,
        string subscription,
        string storageAccount,
        string resourceGroup,
        string reactProject,     // <- This comes after resourceGroup
        RetryPolicyOptions retryPolicy,
        bool build = true,
        string? buildPath = null,
        bool overwrite = true);
}

public sealed record StartupsDeployResources(string StorageAccount, string Container, string Status);
public sealed record StartupsGuidanceInfo(string Title, string Description, string Link);
