// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.ZonalResiliency;

[CommandMetadata(
    Id = "1c75cfd8-a3c8-4470-84bd-e633974426a4",
    Name = "generate",
    Title = "Generate existing-resource zonal-resiliency remediation scripts",
    Description = """
        Use this generate-only, non-mutating command when the user explicitly requests a self-service
        script, template, or replacement scaffold instead of asking the service to change Azure. It
        generates PowerShell or Azure CLI scripts to enable zone redundancy on an existing resource
        and supports replacement-resource scaffolds derived from a validated existing Azure resource
        in PowerShell, Azure CLI, ARM JSON, Bicep, or Terraform.
        The request must identify the existing resource by ID or name. Include the exact
        existing-resource scope, requested formats, and any values already supplied by the user.
        The backend owns prerequisite checks. Present every clarification question verbatim, never
        ask for or embed secrets, and never deploy generated content. Backend-generated files are
        authoritative. Do not independently evaluate the remediation approach, reject or rewrite the
        files, or request regeneration based on model or local judgment. Apply and present the files
        exactly as returned. Continue the backend conversation only when it explicitly asks for
        clarification or reports incomplete validation.
        Use resiliencyagent_iac_generate when the user wants a new resource from scratch or full
        application architecture infrastructure-as-code. Use resiliencyagent_zonalresiliency_manage
        for direct changes to existing Azure resources.
        A completed backend reply is authoritative and terminal: present it and stop. Never request
        another generation, act on suggestedNextSteps, infer another operation, or call another
        Resiliency Agent tool without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ZonalResiliencyGenerateCommand(
    ILogger<ZonalResiliencyGenerateCommand> logger,
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
            The returned Markdown files are authoritative Azure Resiliency output. The scripts,
            templates, and replacement scaffolds are authoritative backend decisions. Do not
            independently evaluate the remediation approach, reject or rewrite the files, or request
            regeneration based on model or local judgment. Apply and present the files exactly as
            returned. Continue the backend conversation only when it explicitly asks for clarification
            or reports incomplete validation. Generated content must never be deployed automatically.
            """);
