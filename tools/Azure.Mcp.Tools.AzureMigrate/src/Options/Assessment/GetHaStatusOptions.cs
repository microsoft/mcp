// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Assessment;

/// <summary>
/// Options for the HA assessment status command.
/// </summary>
public sealed class GetHaStatusOptions
{
    /// <summary>
    /// Gets or sets the HA session ID.
    /// </summary>
    [Option(Description = "Required. ID of the HA session (haSessionId) returned when the assessment was created.")]
    public string? HaSessionId { get; set; }

    /// <summary>
    /// Gets or sets the HA operation ID.
    /// </summary>
    [Option(Description = "Required. ID of the HA operation (haOperationId, a GUID) returned when the assessment was created.")]
    public string? HaOperationId { get; set; }
}
