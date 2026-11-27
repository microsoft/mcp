// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.ServiceGroup;

[CommandMetadata(
    Id = "5238a1f8-58df-4f7e-b56d-db132bbe627e",
    Name = "get",
    Title = "Inspect Azure Service Groups, metadata, criticality, and members",
    Description = """
        Use this inspection command for ordinary Azure Service Group inspection. It can verify whether
        a Service Group is visible, read its name, display name, parent, and criticality, list members,
        summarize member resource types, check whether child Service Groups exist, or perform a
        supported read-only Service Group query. Include the exact Service Group name or ID in request
        whenever one group is intended.
        Returned Service Group and resource-resolution data artifacts are rendered as authoritative
        Markdown files with absolute Azure Portal links. Present the attached artifact and summarize
        it without inventing member data. Continue related turns with conversationId.
        Do not use this command to create or mutate a Service Group and do not use it for automatic
        application discovery, Discovery Agents, saved discoveries, arrangements, or execution plans.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ServiceGroupGetCommand(
    ILogger<ServiceGroupGetCommand> logger,
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
