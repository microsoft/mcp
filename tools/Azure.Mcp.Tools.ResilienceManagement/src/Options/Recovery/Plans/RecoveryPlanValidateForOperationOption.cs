// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanValidateForOperationOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to validate.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The customer-selected recovery operation to validate. Supported values: Failover, FailoverCommit, Reprotect, TestFailover, and TestFailoverCleanup. If omitted, ask the customer to choose one; never infer it from context or resource metadata.")]
    public required string OperationName { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
