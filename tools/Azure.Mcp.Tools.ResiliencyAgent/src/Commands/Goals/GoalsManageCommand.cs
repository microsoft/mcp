// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Goals;

[CommandMetadata(
    Id = "3363b8b5-5a43-4e9b-97d5-16f43a063e9a",
    Name = "manage",
    Title = "Manage an Azure Service Group zonal-resiliency goal",
    Description = """
        Use this command for supported mutations on one Azure Service Group's zonal-resiliency goal:
        assign a goal, optionally assign it with selected Availability or Latency SLIs, delete an
        assignment, attach SLIs after assignment succeeds, include resources, exclude resources with
        an approved exclusion reason, or refresh goal resources. Include the exact Service Group name
        or ID and the user's requested operation in request.
        The backend owns validation and confirmation. Show every elicitation question verbatim and
        never approve a mutation for the user. Goal mutations across multiple Service Groups are not
        supported. Only zonal-resiliency goals are supported. Continue related turns with
        conversationId and present any returned authoritative Markdown artifacts.
        Do not use this command for read-only inspection, Usage Plans, deprecated goal templates, or
        discovery workflows.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class GoalsManageCommand(
    ILogger<GoalsManageCommand> logger,
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
            The returned Markdown files are authoritative Azure Resiliency output. Do not independently
            regenerate, reinterpret, or alter their content. Apply and present the files exactly as
            returned.
            """);
