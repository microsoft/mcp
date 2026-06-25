// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.CollaborationArm;

public class CollaborationCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Cleanroom collaboration resource to create. Must be unique within the resource group.")]
    public required string Name { get; set; }

    [Option(Description = "The Azure region where the collaboration ARM resource will be created (e.g., 'eastus', 'westus2'). This is the RP location.")]
    public required string Location { get; set; }

    [Option(Description = "The Azure region where the cleanroom workload resources (AKS cluster, CACI instances) will be deployed. Defaults to the same as --location if not specified.")]
    public string? ResourceLocation { get; set; }

    [Option(Name = "collaborator", Description = "The email address (userIdentifier) of the collaborator to add at creation time. Can be specified multiple times to add multiple collaborators.")]
    public string[]? Collaborators { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
