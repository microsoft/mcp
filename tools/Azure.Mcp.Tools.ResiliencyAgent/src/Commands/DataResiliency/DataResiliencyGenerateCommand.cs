// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.DataResiliency;

[CommandMetadata(
    Id = "e64f84a8-2f13-4141-9661-6114b6c59055",
    Name = "generate",
    Title = "Generate customer-run Azure Backup automation",
    Description = """
        Use this non-mutating command only when the user explicitly requests customer-run Azure Backup
        automation instead of asking the service to change Azure. Supported output includes Azure CLI,
        native PowerShell, REST, ARM JSON, Bicep, and Terraform. It can generate supported Azure VM
        Backup enablement or configuration automation, short Backup command examples, failed-job
        retrigger scripts, and confirmed vault-deletion scripts.
        Preserve every exact resource, scope, format, setting, and preference the user supplied. The
        backend owns eligibility, source retrieval, composition, validation, clarification, safety
        warnings, and artifact packaging. Present clarification and confirmation questions verbatim.
        Generated files are authoritative backend output. Do not independently rewrite, reject,
        combine, regenerate, execute, or deploy them, and do not claim validation beyond the backend's
        reported validation status.
        Backup enablement automation is currently limited to supported Azure VM Backup scenarios.
        Restore scripts, Azure Site Recovery automation, and non-VM Backup enablement automation are
        unsupported; use resiliencyagent_dataresiliency_get for authoritative guidance and portal
        routing. Use resiliencyagent_dataresiliency_manage when the user wants supported Azure changes
        performed now.
        A completed, declined, cancelled, failed, or rejected backend reply is authoritative and
        terminal: present the authoritativeUserResponse verbatim and stop. Never request another
        generation, act on suggestedNextSteps, infer another operation, or call another Resiliency
        Agent tool without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DataResiliencyGenerateCommand(
    ILogger<DataResiliencyGenerateCommand> logger,
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
            Azure Backup scripts and ARM, Bicep, or Terraform packages are authoritative backend output.
            Preserve every file name and byte-for-byte content. Do not combine surfaces, rewrite files,
            run additional semantic validation, or execute or deploy generated automation.
            """);
