// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Threading.Tasks;
namespace AzureMcp.Areas.Startups.Services;
public interface IStartupsServices
{
    /// <summary>
    /// Gets Microsoft for Startups guidance information.
    /// </summary>
    /// <returns>Guidance information for startups.</returns>
    Task<StartupsGuidanceInfo> GetGuidanceAsync();
    
}
public sealed record StartupsGuidanceInfo(string Title, string Description, string Link);
