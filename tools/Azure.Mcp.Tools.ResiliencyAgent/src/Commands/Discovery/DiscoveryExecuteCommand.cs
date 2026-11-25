// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Discovery;

[CommandMetadata(
    Id = "b7c178fd-8f96-43f6-810b-f15864576057",
    Name = "execute",
    Title = "Execute the latest persisted plan after confirmation",
    Description = """
        Execute the latest persisted Discovery Service Group arrangement and ask for explicit
        confirmation before writing Azure. Execute now or apply now. This execution command deploys
        and commits final Azure writes from reviewed Discovery work. Use only when the user
        explicitly asks to execute, apply, deploy, or commit the final result. The backend validates
        the latest persisted revision. The initial
        request is routing intent, not approval, and declined or unavailable confirmation stops
        without mutation.
        Treat the backend reply as authoritative. A completed, declined, or cancelled response is
        terminal: present it and stop. Do not repeat execution, act on suggestedNextSteps, or invoke
        another Discovery command without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DiscoveryExecuteCommand(
    ILogger<DiscoveryExecuteCommand> logger,
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
