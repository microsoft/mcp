// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans.Enrollments;

public class UsagePlanEnrollmentCreateOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the usage plan.")]
    public required string UsagePlan { get; set; }

    [Option(Description = "The name of the usage plan enrollment.")]
    public required string Enrollment { get; set; }

    [Option(Description = "The name of the service group to associate with the enrollment.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
