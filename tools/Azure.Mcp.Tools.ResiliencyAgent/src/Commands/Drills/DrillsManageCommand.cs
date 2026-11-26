// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Drills;

[CommandMetadata(
    Id = "3e4ccadb-c814-4af9-80ba-7c8fdfd18713",
    Name = "manage",
    Title = "Create or configure an Azure resilience drill",
    Description = """
        Use the conversational Stay Resilient drills workflow for a complete guided drill
        configuration change. It can create a zone-down drill; update monitoring, RBAC setup,
        supporting-resource settings, goal identity, fault duration, or drill resources; and add
        customer-provided notes to a drill run. It can orchestrate required Service Group
        prerequisites, including a zonal resilience goal, Standard Usage Plan enrollment, and a
        Recovery Plan when virtual machines require one.
        The backend re-reads the saved drill and Service Group, resolves prerequisites, presents the
        exact proposed change, and performs a write only after the customer's plain confirmation.
        A fully specified request remains eligible: supplied values are inputs, not authorization to
        bypass validation or confirmation. Forward every exact name, resource ID, region, setting,
        monitoring choice, and note the user supplied. Never approve a change for the user.
        This command configures drills but does not execute or delete them. Use
        resiliencyagent_drills_get for inspection, readiness checks, validation, diagnosis, and
        portal guidance.
        A completed, declined, cancelled, failed, or rejected backend reply is authoritative and
        terminal: present authoritativeUserResponse verbatim and stop. Never retry a mutation, act
        on suggestedNextSteps, or call another Resiliency Agent tool without a new explicit request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DrillsManageCommand(
    ILogger<DrillsManageCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null)
    : ResiliencyAgentCommand<ResiliencyAgentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache,
        timeProvider,
        artifactInstructions: """
            A returned FullList.md file is authoritative Stay Resilient output. Preserve every row,
            prerequisite, proposed change, warning, status, and Azure Portal link exactly as returned.
            Do not infer approval or replace the complete file with a shorter reconstruction.
            """);
