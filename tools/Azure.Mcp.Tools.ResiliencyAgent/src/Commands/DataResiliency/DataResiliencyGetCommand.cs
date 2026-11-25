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
    Title = "Assess Backup coverage and eligibility or provide recovery guidance",
    Description = """
        Evaluate Azure VM Backup eligibility for one or more resource IDs without changing Azure.
        Explain restore options and provide the verified recovery portal route without restoring a
        soft-deleted item or executing recovery. Help configure Azure Site Recovery, Azure Files Backup, and other
        non-VM Backup workloads through authoritative portal guidance rather than native enablement.
        Assess Azure Backup protected-versus-unprotected coverage across subscriptions, resource
        groups, resource criteria, or Service Groups with complete reports and charts. Analyze
        Backup and ASR alerts, jobs, vaults, policies, protected items, recovery-point health,
        retention, and security posture, including evidence-backed failed-job diagnosis.
        Use this agent-backed read and guidance command when interpretation, aggregation,
        recommendations, clarification, tables, reports, charts, or guidance are the outcome.
        Forward exact identifiers and requirements already supplied.
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
