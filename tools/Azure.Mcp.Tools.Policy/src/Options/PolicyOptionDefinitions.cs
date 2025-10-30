// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Options;

public static class PolicyOptionDefinitions
{
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
}
