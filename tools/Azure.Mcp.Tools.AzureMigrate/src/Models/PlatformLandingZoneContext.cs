// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Identifies a single Platform Landing Zone resource under an Azure Migrate project.
/// </summary>
/// <param name="SubscriptionId">The subscription holding the migrate project.</param>
/// <param name="ResourceGroupName">The resource group holding the migrate project.</param>
/// <param name="MigrateProjectName">The migrate project name.</param>
/// <param name="LandingZoneName">The Platform Landing Zone resource name.</param>
public sealed record PlatformLandingZoneContext(
    string SubscriptionId,
    string ResourceGroupName,
    string MigrateProjectName,
    string LandingZoneName);
