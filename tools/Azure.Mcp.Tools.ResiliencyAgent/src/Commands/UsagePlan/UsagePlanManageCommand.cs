// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.UsagePlan;

[CommandMetadata(
    Id = "71602af2-2a34-47fd-a169-d123375cbb6b",
    Name = "manage",
    Title = "Create or attach an Azure resiliency Standard Usage Plan",
    Description = """
        Use this command only when the user explicitly asks to create a Standard Azure Resilience
        Management Usage Plan or attach an existing Standard plan to one Service Group. Include the
        exact subscription ID, resource group, plan name, Service Group, and whether a missing
        resource group should be created in request when known.
        The backend must collect missing values and obtain explicit confirmation before any write.
        Never invent a plan name, subscription, resource group, or location, and never approve an
        elicitation for the user. Basic Usage Plans are retired and cannot be created or attached.
        Usage Plans are required only for premium recovery-plan and drill capabilities; goals and
        zonal-resiliency insights do not require one. Continue related turns with conversationId.
        Do not use this command for goal assignment or Service Group discovery.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class UsagePlanManageCommand(
    ILogger<UsagePlanManageCommand> logger,
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
