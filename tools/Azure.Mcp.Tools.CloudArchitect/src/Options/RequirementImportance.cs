// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.CloudArchitect.Options;

/// <summary>
/// Represents the importance level of a requirement.
/// </summary>
[JsonConverter(typeof(RequirementImportanceConverter))]
public enum RequirementImportance
{
    High,
    Medium,
    Low
}
