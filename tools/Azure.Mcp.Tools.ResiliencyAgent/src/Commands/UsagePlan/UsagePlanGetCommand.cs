// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.UsagePlan;

[CommandMetadata(
    Id = "5c6ae03b-4356-4ce9-82b4-876f4945f983",
    Name = "get",
    Title = "Inspect Azure resiliency Usage Plans and enrollments",
    Description = """
        Use this inspection command only when the user explicitly asks about Azure Resilience
        Management Usage Plans or Service Group enrollment. It can list or find plans, inspect the
        observed plan type and status, check one Service Group's enrollment, and find eligible
        Standard plans for attachment. Include exact subscription, resource group, plan name, Service
        Group, or other filters in request.
        Standard is the only supported plan type for creation and attachment. Existing Basic records
        may be reported factually as retired and unsupported, but must never be offered as attachment
        candidates. Goals, recommendations, and zonal-resiliency insights do not require a Usage Plan.
        Continue related turns with conversationId.
        Do not use this command to create or attach a plan; use resiliencyagent_usage-plan_manage.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class UsagePlanGetCommand(
    ILogger<UsagePlanGetCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null)
    : ResiliencyAgentCommand<ResiliencyAgentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache,
        timeProvider);
