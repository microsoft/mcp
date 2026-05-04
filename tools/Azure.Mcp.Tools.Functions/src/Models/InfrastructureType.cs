// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Infrastructure type for Azure Functions templates.
/// Templates with a value other than <see cref="None"/> include infrastructure files and are azd-ready for deployment.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<InfrastructureType>))]
public enum InfrastructureType
{
    /// <summary>Code-only template, no infrastructure files.</summary>
    [JsonStringEnumMemberName("none")]
    None,

    /// <summary>Includes Bicep infrastructure files, azd-ready.</summary>
    [JsonStringEnumMemberName("bicep")]
    Bicep,

    /// <summary>Includes Terraform infrastructure files, azd-ready.</summary>
    [JsonStringEnumMemberName("terraform")]
    Terraform,

    /// <summary>Includes ARM template infrastructure files, azd-ready.</summary>
    [JsonStringEnumMemberName("arm")]
    Arm
}
