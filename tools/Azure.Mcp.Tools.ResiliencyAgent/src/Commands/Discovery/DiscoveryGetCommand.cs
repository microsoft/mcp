// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Discovery;

[CommandMetadata(
    Id = "d03383f6-25aa-49d2-8eff-77f6ce427d5e",
    Name = "get",
    Title = "Inspect Discovery Agents, access, saved results, and plans",
    Description = """
        List the available Azure application Discovery Agents for a subscription. Check whether a
        configured Discovery Agent has Reader access for a subscription or resource group without
        assigning access. List previous saved Azure application discoveries. Show a selected
        application-resource boundary report. Display the current saved Service Group arrangement
        for a customer-visible Plan Group ID and revision without applying changes.
        Display the current unchanged plan preview for a Plan Group ID and revision.
        Display the current saved Service Group arrangement for the requested Plan Group revision.
        Use this inspection command only for existing Discovery information. Do not create an agent,
        assign Reader access, run fresh discovery, change an arrangement, or execute a plan.
        Treat the backend reply as authoritative. A completed response is terminal: present it and
        stop. Do not repeat the call, act on suggestedNextSteps, choose a discovery reference, infer
        another operation, or invoke another Discovery command without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DiscoveryGetCommand(
    ILogger<DiscoveryGetCommand> logger,
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
            Backend-authored Discovery Markdown artifact content is authoritative. Preserve content
            exactly, including Portal-specific :::card and :::::accordion directives. Use the local
            filename and workspace link returned by Azure MCP. The writer may apply standard path,
            whitespace, control-character, and invalid-character sanitization; do not otherwise
            rename, transcode, rewrite, re-evaluate, reject, or replace the artifact.
            """);
