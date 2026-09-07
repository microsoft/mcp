// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Constants;

/// <summary>
/// Constants for Platform Landing Zone operations and guidance.
/// </summary>
internal static class PlatformLandingZoneConstants
{
    /// <summary>
    /// Base URL for Azure Landing Zone scenario documentation.
    /// </summary>
    public const string ScenarioDocsBaseUrl = "https://aka.ms/azmcp/lzscenariodocs";

    /// <summary>
    /// Base URL for Azure Landing Zone archetype definitions.
    /// </summary>
    public const string ArchetypeDefinitionsBaseUrl = "https://aka.ms/azmcp/lzarchetypedefs";

    /// <summary>
    /// API version for the Platform Landing Zone ARM resource
    /// (<c>Microsoft.Migrate/migrateProjects/platformLandingZones</c>).
    /// </summary>
    public const string PlatformLandingZoneApiVersion = "2026-02-01-preview";

    /// <summary>
    /// API version for the Artifact Store ARM resource
    /// (<c>Microsoft.Migrate/migrateProjects/artifacts</c>). Deliberately different from
    /// <see cref="PlatformLandingZoneApiVersion"/>: the two resource types version independently.
    /// </summary>
    public const string ArtifactApiVersion = "2026-06-01-preview";

    /// <summary>
    /// Default Platform Landing Zone resource name when the caller does not supply one.
    /// </summary>
    public const string DefaultLandingZoneName = "default";

    /// <summary>
    /// Prefix applied to the Artifact Store artifact backing a Platform Landing Zone.
    /// </summary>
    public const string ArtifactNamePrefix = "plz-";

    /// <summary>
    /// Name of the committed artifact file holding the generated infrastructure-as-code output.
    /// </summary>
    public const string OutputZipFileName = "output.zip";

    /// <summary>
    /// Name of the committed artifact file holding the rendered design document.
    /// </summary>
    public const string DesignDocumentFileName = "design-document.md";

    /// <summary>
    /// Generation status indicating the run is still in flight.
    /// </summary>
    public const string StatusRunning = "Running";

    /// <summary>
    /// Generation status indicating the run completed successfully.
    /// </summary>
    public const string StatusSucceeded = "Succeeded";

    /// <summary>
    /// Generation status indicating the run failed.
    /// </summary>
    public const string StatusFailed = "Failed";

    /// <summary>
    /// Default overall timeout applied when waiting for a generation run to reach a terminal status.
    /// </summary>
    public static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Interval between polls while waiting for a generation run to reach a terminal status.
    /// </summary>
    public static readonly TimeSpan WaitPollInterval = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Cache expiration time for policy location data.
    /// </summary>
    public static readonly TimeSpan PolicyCacheExpiry = TimeSpan.FromHours(6);

    /// <summary>
    /// Archetype definition file names.
    /// </summary>
    public static readonly string[] ArchetypeDefinitionFiles =
    [
        "connectivity.alz_archetype_definition.json",
        "corp.alz_archetype_definition.json",
        "decommissioned.alz_archetype_definition.json",
        "identity.alz_archetype_definition.json",
        "landing_zones.alz_archetype_definition.json",
        "management.alz_archetype_definition.json",
        "online.alz_archetype_definition.json",
        "platform.alz_archetype_definition.json",
        "root.alz_archetype_definition.json",
        "sandbox.alz_archetype_definition.json",
        "security.alz_archetype_definition.json"
    ];
}
