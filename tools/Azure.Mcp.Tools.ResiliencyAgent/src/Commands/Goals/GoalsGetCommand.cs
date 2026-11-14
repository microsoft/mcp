// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Goals;

[CommandMetadata(
    Id = "9da365c8-dd3b-486d-9822-b445c67f0a03",
    Name = "get",
    Title = "Inspect Azure resiliency goals and goal-resource participation",
    Description = """
        Use this inspection command for Azure Service Group zonal-resiliency goal information. It can
        get one Service Group's goal-assignment state or object, show assignment status across all
        visible Service Groups, inspect goal-resource inclusion and exclusion status, list attached
        or available Availability and Latency SLIs, and answer supported goal-data queries. Include
        the exact Service Group name or ID and requested filters in request. Cross-Service Group
        status reads are supported; mutations across multiple Service Groups are not.
        Returned DataGrid and Resiliency Agent data artifacts are rendered as authoritative Markdown
        files with absolute Azure Portal links. Present the attached Markdown artifact and summarize
        it without fabricating rows. Continue related turns with conversationId.
        Do not use this command to assign, delete, include, exclude, refresh, or attach SLIs; use
        resiliencyagent_goals_manage. Do not use it for Usage Plans or discovery workflows.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class GoalsGetCommand(
    ILogger<GoalsGetCommand> logger,
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
