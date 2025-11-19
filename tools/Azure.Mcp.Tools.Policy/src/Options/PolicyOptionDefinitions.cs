// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

<<<<<<< HEAD
using Azure.Mcp.Tools.Policy.Models;
=======
using System.CommandLine;
>>>>>>> main

namespace Azure.Mcp.Tools.Policy.Options;

public static class PolicyOptionDefinitions
{
<<<<<<< HEAD
    public static class Assignment
    {
        public const string AssignmentName = "assignment";
        public static readonly Option<string> AssignmentOption = new(
            $"--{AssignmentName}"
        )
        {
            Description = "The name of the policy assignment.",
            Required = false
        };

        public const string PolicyAssignmentsName = "policy-assignments";
        public static readonly Option<PolicyAssignment[]> PolicyAssignmentsOption = new(
            $"--{PolicyAssignmentsName}"
        )
        {
            Description = "The policy assignments retrieved from using the Azure CLI tool.",
            Required = false
        };
    }
=======
    public const string AssignmentNameOption = "assignment-name";
    public const string DefinitionNameOption = "definition-name";
    public const string ScopeOption = "scope";
    public const string ManagementGroupOption = "management-group";

    public static readonly Option<string> AssignmentName = new(
        $"--{AssignmentNameOption}"
    )
    {
        Description = "The name of the policy assignment to retrieve.",
        Required = true
    };

    public static readonly Option<string> DefinitionName = new(
        $"--{DefinitionNameOption}"
    )
    {
        Description = "The name of the policy definition to retrieve.",
        Required = true
    };

    public static readonly Option<string> Scope = new(
        $"--{ScopeOption}"
    )
    {
        Description = "The scope of the policy assignment (e.g., /subscriptions/{subscriptionId}, /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}).",
        Required = true
    };

    public static readonly Option<string> ManagementGroup = new(
        $"--{ManagementGroupOption}"
    )
    {
        Description = "The management group ID to retrieve the policy definition from. If specified, retrieves management group-level policy definitions instead of subscription-level.",
        Required = false
    };
>>>>>>> main
}
