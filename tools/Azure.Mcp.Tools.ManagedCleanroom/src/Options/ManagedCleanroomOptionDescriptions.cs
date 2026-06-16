// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedCleanroom.Options;

/// <summary>
/// Description constants for Azure Managed Cleanroom options shared across commands.
/// Options are defined inline via <see cref="Microsoft.Mcp.Core.Options.OptionAttribute"/> on each options class.
/// </summary>
public static class ManagedCleanroomOptionDescriptions
{
    public const string Endpoint =
        "The Azure Cleanroom Analytics Frontend service endpoint URL (e.g., 'https://my-cleanroom.cloudapp.azure.net').";

    public const string CollaborationId =
        "The unique identifier (UUID) of the cleanroom collaboration.";

    public const string TokenScope =
        "Optional Microsoft Entra token scope for the cleanroom frontend API (for example, 'https://my-cleanroom.cloudapp.azure.net/.default'). Defaults to '<endpoint-origin>/.default'.";

    public const string DocumentId =
        "The unique identifier (UUID) of the dataset document to publish.";
}
