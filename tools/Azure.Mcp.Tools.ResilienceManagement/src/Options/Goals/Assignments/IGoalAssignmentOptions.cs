// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public interface IGoalAssignmentOptions
{
    string ServiceGroup { get; }
    string GoalAssignment { get; }
}
