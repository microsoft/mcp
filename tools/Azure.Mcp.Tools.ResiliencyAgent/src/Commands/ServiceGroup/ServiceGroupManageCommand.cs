// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.ServiceGroup;

[CommandMetadata(
    Id = "2018f499-3e9f-48e2-9cbd-00e3db9ef0f7",
    Name = "manage",
    Title = "Create or update an Azure Service Group",
    Description = """
        Use this command for supported ordinary Azure Service Group mutations: create a root or nested
        Service Group, optionally set criticality and validated initial resources, add validated
        resources by Azure Resource Graph criteria or exact ARM IDs, add a subscription or resource
        group as one direct scope, nest an existing Service Group, or update criticality. Include the
        exact target name, placement, resources or criteria, and requested criticality in request when
        known.
        The backend owns name validation, existence checks, resource validation, limits, and explicit
        confirmation. Show every elicitation question verbatim and never approve a write for the user.
        Direct ordinary member removal is not supported. Continue related turns with conversationId
        and present any returned authoritative Markdown artifacts.
        Do not use this command for automatic application discovery, Discovery Agents, saved
        discoveries, arrangements, or multi-Service-Group discovery-plan execution.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ServiceGroupManageCommand(
    ILogger<ServiceGroupManageCommand> logger,
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
