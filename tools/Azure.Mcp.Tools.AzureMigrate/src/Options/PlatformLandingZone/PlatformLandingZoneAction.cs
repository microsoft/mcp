// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>Actions supported by the platform landing zone request command.</summary>
public enum PlatformLandingZoneAction
{
    /// <summary>Create an Azure Migrate project.</summary>
    [JsonStringEnumMemberName("createmigrateproject")]
    CreateMigrateProject,

    /// <summary>Update platform landing zone parameters.</summary>
    [JsonStringEnumMemberName("update")]
    Update,

    /// <summary>Check whether a platform landing zone exists.</summary>
    [JsonStringEnumMemberName("check")]
    Check,

    /// <summary>Generate a platform landing zone.</summary>
    [JsonStringEnumMemberName("generate")]
    Generate,

    /// <summary>Download generated platform landing zone files.</summary>
    [JsonStringEnumMemberName("download")]
    Download,

    /// <summary>Get the current platform landing zone parameter status.</summary>
    [JsonStringEnumMemberName("status")]
    Status
}
