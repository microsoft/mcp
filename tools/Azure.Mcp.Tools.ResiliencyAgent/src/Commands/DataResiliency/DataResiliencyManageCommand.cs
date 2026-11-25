// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.DataResiliency;

[CommandMetadata(
    Id = "4d74ac37-294f-4119-b457-2378dabbeb38",
    Name = "manage",
    Title = "Orchestrate guided Azure Backup changes",
    Description = """
        Use this destructive agent-backed command for supported guided Azure Backup workflows that
        require eligibility checks, compatibility validation, missing-value collection, dependent
        sequencing, or backend confirmation. It can guide Azure VM Backup enablement through
        eligibility, compatible vault and policy selection or creation, disk selection, confirmation,
        protection, and outcome verification. It also supports guided vault, policy, protection,
        undelete, security-posture, Resource Guard, role-assignment, encryption, networking, and other
        backend-supported Azure Backup management workflows.
        Forward every exact resource, vault, policy, setting, and preference the user already supplied.
        A fully specified request remains eligible for this workflow: supplied values are inputs and
        intent, not authorization to skip backend validation or confirmation. Show every backend
        elicitation question verbatim and never approve a write for the user.
        Native protection enablement is limited to supported Azure VM Backup scenarios. Non-VM Backup
        enablement, Azure Site Recovery configuration or recovery, and restore execution belong to
        resiliencyagent_dataresiliency_get for authoritative guidance and portal routing. Use
        resiliencyagent_dataresiliency_generate when the requested outcome is customer-run automation
        rather than an Azure change now.
        A completed, declined, cancelled, failed, or rejected backend reply is authoritative and
        terminal: present the authoritativeUserResponse verbatim and stop. Never retry a mutation, act
        on suggestedNextSteps, infer another operation, or call another Resiliency Agent tool without
        a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DataResiliencyManageCommand(
    ILogger<DataResiliencyManageCommand> logger,
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
            Backend-authored eligibility and configuration reports are authoritative. Preserve every
            resource row, decision, status, warning, link, and generated file exactly as returned.
            Never treat a report, suggested next step, or initial request as user confirmation.
            """);
