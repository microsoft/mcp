// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Startups.Services;

public interface IStartupsService
{
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
}

public sealed record StartupsDeployResources(string StorageAccount, string Container, string Status, string WebsiteUrl, string PortalUrl, string ContainerUrl);
public sealed record StartupsGuidanceInfo(string Title, string Description, string Link);
