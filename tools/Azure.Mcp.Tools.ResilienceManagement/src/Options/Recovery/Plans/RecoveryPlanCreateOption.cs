// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanCreateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to create or fully update.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The recovery plan type. Supported value: Zonal. The type cannot be changed after creation.")]
    public required RecoveryPlanKind PlanType { get; set; }

    [Option(Description =
        "The recovery plan description, from 5 to 50 characters. " +
        "Required when creating a plan; on update, the existing description is preserved when omitted.")]
    public string? PlanDescription { get; set; }

    [Option(Description =
        "The customer-selected managed identity type for the recovery plan. " +
        "Supported values: SystemAssigned, UserAssigned, and SystemAndUserAssigned. " +
        "Do not assume a default; ask the customer when they have not specified an identity type. " +
        "Specify this on every create or update; updates can switch identity types, but cannot replace an existing " +
        "user-assigned identity with a different user-assigned identity.")]
    public required RecoveryPlanIdentityKind IdentityType { get; set; }

    [Option(Description =
        "The full resource ID of the user-assigned managed identity. " +
        "Required when --identity-type is UserAssigned or SystemAndUserAssigned and not allowed when it is SystemAssigned. " +
        "On update, specify the existing user-assigned identity because changing it to a different user-assigned identity " +
        "is not supported.")]
    public string? UserAssignedIdentity { get; set; }

    [Option(Description = "The default recovery group description, from 5 to 50 characters. On update, the existing description is preserved when omitted.")]
    public string? DefaultGroupDescription { get; set; }

    [Option(Description =
        "A JSON array that replaces the default recovery group's pre-actions. " +
        "Before invoking the tool, collect and explain these values to the customer one at a time: " +
        "(1) type: ManualAction pauses for a person to complete a step; CustomRunbook runs an Azure Automation runbook; " +
        "(2) name: a 3 to 24 character customer-facing action name containing only letters, numbers, or hyphens; " +
        "(3) description: optional action instructions up to 100 characters; an empty value is allowed; " +
        "(4) timeoutInMinutes: a positive whole number defining how long the action may run; " +
        "(5) actionResourceId: required only for CustomRunbook and must be the full resource ID of a " +
        "Microsoft.Automation/automationAccounts/runbooks resource; " +
        "(6) parameters: optional or null for CustomRunbook; when provided, it must be a JSON object whose values are strings. " +
        "ManualAction example: [{\"type\":\"ManualAction\",\"name\":\"Confirm-dependencies\",\"description\":\"Verify dependencies are ready\",\"timeoutInMinutes\":30}]. " +
        "CustomRunbook example: [{\"type\":\"CustomRunbook\",\"name\":\"Start-dependencies\",\"description\":\"Start application dependencies\",\"timeoutInMinutes\":30,\"actionResourceId\":\"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Automation/automationAccounts/{account}/runbooks/{runbook}\",\"parameters\":{\"environment\":\"production\"}}]. " +
        "Omit this option to preserve existing pre-actions; use an empty array to clear them.")]
    public string? DefaultGroupPreActions { get; set; }

    [Option(Description =
        "A JSON array that replaces the default recovery group's post-actions. " +
        "Before invoking the tool, collect and explain these values to the customer one at a time: " +
        "(1) type: ManualAction pauses for a person to complete a step; CustomRunbook runs an Azure Automation runbook; " +
        "(2) name: a 3 to 24 character customer-facing action name containing only letters, numbers, or hyphens; " +
        "(3) description: optional action instructions up to 100 characters; an empty value is allowed; " +
        "(4) timeoutInMinutes: a positive whole number defining how long the action may run; " +
        "(5) actionResourceId: required only for CustomRunbook and must be the full resource ID of a " +
        "Microsoft.Automation/automationAccounts/runbooks resource; " +
        "(6) parameters: optional or null for CustomRunbook; when provided, it must be a JSON object whose values are strings. " +
        "ManualAction example: [{\"type\":\"ManualAction\",\"name\":\"Confirm-recovery\",\"description\":\"Verify recovery completed successfully\",\"timeoutInMinutes\":30}]. " +
        "CustomRunbook example: [{\"type\":\"CustomRunbook\",\"name\":\"Validate-recovery\",\"description\":\"Run post-recovery validation\",\"timeoutInMinutes\":30,\"actionResourceId\":\"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Automation/automationAccounts/{account}/runbooks/{runbook}\",\"parameters\":{\"environment\":\"production\"}}]. " +
        "Omit this option to preserve existing post-actions; use an empty array to clear them.")]
    public string? DefaultGroupPostActions { get; set; }

    [Option(Description =
        "A JSON array that replaces the additional recovery groups. " +
        "Before invoking the tool, collect and explain these group values to the customer one at a time: " +
        "(1) orderId: a unique whole number from 1 to 14; additional groups must be sequential starting at 1; " +
        "(2) description: customer-facing text from 5 to 50 characters; " +
        "(3) groupUniqueId: optional group GUID; omit it to preserve the existing group ID at that order or generate a GUID for a new group; " +
        "(4) preActions and postActions: optional action arrays. For each action, collect type, a 3 to 24 character name " +
        "containing only letters, numbers, or hyphens, optional action instructions up to 100 characters (empty is allowed), " +
        "positive timeoutInMinutes, and, for CustomRunbook, the Automation runbook actionResourceId and optional or null string-valued parameters. " +
        "ManualAction pauses for a person to complete a step; CustomRunbook runs an Azure Automation runbook. " +
        "Example: [{\"orderId\":1,\"description\":\"Application recovery group\",\"preActions\":[{\"type\":\"ManualAction\",\"name\":\"Confirm-dependencies\",\"description\":\"Verify dependencies are ready\",\"timeoutInMinutes\":30}]}]. " +
        "Omit preActions or postActions to preserve that action list; use an empty array to clear it. " +
        "Omit this option to preserve all existing additional groups. " +
        "Use an empty array to remove all additional groups.")]
    public string? AdditionalGroups { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer<RetryPolicyOptions>(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
