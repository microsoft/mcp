// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmCreateResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("vmSize")] string? VmSize,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("osType")] string? OsType,
    [property: JsonPropertyName("publicIpAddress")] string? PublicIpAddress,
    [property: JsonPropertyName("privateIpAddress")] string? PrivateIpAddress,
    [property: JsonPropertyName("zones")] IReadOnlyList<string>? Zones,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags,
    [property: JsonPropertyName("workloadConfiguration")] WorkloadConfiguration? WorkloadConfiguration);

public sealed record WorkloadConfiguration(
    [property: JsonPropertyName("workloadType")] string WorkloadType,
    [property: JsonPropertyName("suggestedVmSize")] string SuggestedVmSize,
    [property: JsonPropertyName("suggestedOsDiskType")] string SuggestedOsDiskType,
    [property: JsonPropertyName("suggestedOsDiskSizeGb")] int SuggestedOsDiskSizeGb,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("requirements")] string? Requirements = null);

/// <summary>
/// Requirements for Windows VMs:
/// - Computer name cannot be more than 15 characters long
/// - Computer name cannot be entirely numeric
/// - Computer name cannot contain the following characters: ` ~ ! @ # $ % ^ &amp; * ( ) = + _ [ ] { } \ | ; : . ' " , &lt; &gt; / ?
/// </summary>
public static class VmRequirements
{
    public const string WindowsComputerName = "Windows computer name cannot be more than 15 characters long, be entirely numeric, or contain special characters (` ~ ! @ # $ % ^ & * ( ) = + _ [ ] { } \\ | ; : . ' \" , < > / ?).";
}
