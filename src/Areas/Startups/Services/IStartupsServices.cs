// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Threading.Tasks;
using Azure.ResourceManager.Storage.Models;
using AzureMcp.Areas.Startups.Options;
namespace AzureMcp.Areas.Startups.Services;

public interface IStartupsServices
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

public sealed record StartupsDeployResources(string storageAccount, string container, string status);
public sealed record StartupsGuidanceInfo(string Title, string Description, string Link);
