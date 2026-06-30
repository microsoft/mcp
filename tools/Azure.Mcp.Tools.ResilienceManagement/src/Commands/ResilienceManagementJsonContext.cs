// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Templates;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands;

[JsonSerializable(typeof(GoalTemplateGetCommand.GoalTemplateGetCommandResult))]
[JsonSerializable(typeof(GoalTemplateInfo))]
[JsonSerializable(typeof(GoalTemplateInfoProperties))]
[JsonSerializable(typeof(GoalTemplateInfoSystemData))]
[JsonSerializable(typeof(GoalAssignmentGetCommand.GoalAssignmentGetCommandResult))]
[JsonSerializable(typeof(GoalAssignmentInfo))]
[JsonSerializable(typeof(GoalAssignmentInfoProperties))]
[JsonSerializable(typeof(GoalAssignmentInfoSystemData))]
[JsonSerializable(typeof(GoalResourceGetCommand.GoalResourceGetCommandResult))]
[JsonSerializable(typeof(GoalResourceInfo))]
[JsonSerializable(typeof(GoalResourceInfoProperties))]
[JsonSerializable(typeof(GoalResourceServiceGroupMembership))]
[JsonSerializable(typeof(GoalResourceInfoSystemData))]
[JsonSerializable(typeof(UsagePlanGetCommand.UsagePlanGetCommandResult))]
[JsonSerializable(typeof(UsagePlanInfo))]
[JsonSerializable(typeof(UsagePlanInfoProperties))]
[JsonSerializable(typeof(UsagePlanInfoSystemData))]
[JsonSerializable(typeof(UsagePlanEnrollmentGetCommand.UsagePlanEnrollmentGetCommandResult))]
[JsonSerializable(typeof(UsagePlanEnrollmentInfo))]
[JsonSerializable(typeof(UsagePlanEnrollmentInfoProperties))]
[JsonSerializable(typeof(UsagePlanEnrollmentInfoErrorDetails))]
[JsonSerializable(typeof(UsagePlanEnrollmentInfoSystemData))]
[JsonSerializable(typeof(RecoveryPlanGetCommand.RecoveryPlanGetCommandResult))]
[JsonSerializable(typeof(RecoveryResourceGetCommand.RecoveryResourceGetCommandResult))]
[JsonSerializable(typeof(RecoveryJobGetCommand.RecoveryJobGetCommandResult))]
[JsonSerializable(typeof(RecoveryJobResourceGetCommand.RecoveryJobResourceGetCommandResult))]
[JsonSerializable(typeof(ResourceSummary))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class ResilienceManagementJsonContext : JsonSerializerContext
{
}
