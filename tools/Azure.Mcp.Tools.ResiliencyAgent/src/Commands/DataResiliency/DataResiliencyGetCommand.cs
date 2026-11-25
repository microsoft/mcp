// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.DataResiliency;

[CommandMetadata(
    Id = "0ccb4c3e-ed15-40bd-9e58-da5217bfbc0a",
    Name = "get",
    Title = "Assess Azure Backup and recovery posture",
    Description = """
        Use this agent-backed conversational command for broad Azure Backup and Azure Site Recovery
        assessment, aggregation, explanation, and guidance. It can assess protected-versus-unprotected
        coverage for subscriptions, resource groups, resource criteria, or Service Groups; inspect and
        analyze alerts, jobs, vaults, policies, protected items, recovery-point health, retention, and
        security posture; evaluate Azure VM Backup eligibility without making changes; troubleshoot
        failed backup jobs from returned evidence; and provide restore, non-VM Backup enablement, and
        Azure Site Recovery portal guidance.
        Use it when the user wants the Data Resiliency Agent's interpretation, recommendations,
        cross-resource aggregation, missing-value clarification, reports, charts, or multi-turn
        guidance. Exact identifiers and fully specified requirements should be forwarded when the user
        supplies them; the backend remains authoritative for scope and capability boundaries.
        This command does not change Azure and does not generate customer-run scripts or templates.
        Use resiliencyagent_dataresiliency_manage for supported guided Azure Backup changes and
        resiliencyagent_dataresiliency_generate for explicit automation generation.
        Backend responses, tables, reports, charts, links, and guidance are authoritative. A completed
        backend reply is terminal: present the authoritativeUserResponse verbatim and stop. Never retry,
        act on suggestedNextSteps, infer another operation, or call another Resiliency Agent tool
        without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DataResiliencyGetCommand(
    ILogger<DataResiliencyGetCommand> logger,
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
            Backend-authored Azure Backup and recovery tables, reports, charts, and Markdown files are
            authoritative. Preserve their complete content, ordering, counts, links, and truncation
            disclosures. Do not regenerate, reinterpret, summarize away, or replace them.
            """);
