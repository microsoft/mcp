// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmCreateResult(
    string Name,
    string? Id,
    string? Location,
    string? VmSize,
    string? ProvisioningState,
    string? OsType,
    string? PublicIpAddress,
    string? PrivateIpAddress,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);

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
