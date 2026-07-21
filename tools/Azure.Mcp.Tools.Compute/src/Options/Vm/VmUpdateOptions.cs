// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public sealed class VmUpdateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmName, Aliases = ["name"])]
    public required string VmName { get; set; }

    [Option(Description = ComputeOptionDescriptions.VmSize, Aliases = ["size"])]
    public string? VmSize { get; set; }

    [Option(Description = ComputeOptionDescriptions.TagsUpdate, AllowEmptyOrWhiteSpaceString = true)]
    public string? Tags { get; set; }

    [Option(Description = "License type for Azure Hybrid Benefit: 'Windows_Server', 'Windows_Client', 'RHEL_BYOS', 'SLES_BYOS', or 'None' to disable")]
    public string? LicenseType { get; set; }

    [Option(Description = "Enable or disable boot diagnostics.")]
    public string? BootDiagnostics { get; set; }

    [Option(Description = "Base64-encoded user data for the VM (e.g., a cloud-init or shell script). The value must be Base64-encoded before passing; the ARM API requires this format.")]
    public string? UserData { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
