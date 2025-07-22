// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Options;
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
    Task<StartupsDeployResources> DeployStaticWebAsync(StartupsDeployOptions options, CancellationToken cancellationToken);

}

public sealed record StartupsDeployResources(string StorageAccount, string Container, string Status);
public sealed record StartupsGuidanceInfo(string Title, string Description, string Link);
