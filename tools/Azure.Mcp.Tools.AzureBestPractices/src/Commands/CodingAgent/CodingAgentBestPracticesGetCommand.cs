// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Azure.Mcp.Tools.AzureBestPractices.Options.CodingAgent;
using Azure.Mcp.Tools.AzureBestPractices.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands.CodingAgent;

[CommandMetadata(
    Id = "3f7c1a52-9d84-4e6b-b0a1-6c2f8e5d47a3",
    Name = "get_coding_agent",
    Title = "Get coding-agent Azure best practices",
    Description = """
        Returns the focused best-practices guide for coding agents working with Azure.
        Use this whenever an AI coding agent is asked to generate, review, deploy or
        operate Azure code, including Azure Functions, AKS, Container Apps, Bicep,
        Terraform and Azure SDK usage. The result is a curated Markdown document with
        security, reliability and deployment guidance.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CodingAgentBestPracticesGetCommand(
    ILogger<CodingAgentBestPracticesGetCommand> logger,
    IAzureBestPracticesService service)
    : BaseCommand<CodingAgentBestPracticesOptions, CodingAgentBestPracticesGetCommand.CodingAgentBestPracticesGetCommandResult>
{
    private readonly ILogger<CodingAgentBestPracticesGetCommand> _logger = logger;
    private readonly IAzureBestPracticesService _service = service;

    public override JsonTypeInfo<CodingAgentBestPracticesGetCommandResult> ResultTypeInfo =>
        AzureBestPracticesJsonContext.Default.CodingAgentBestPracticesGetCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        CodingAgentBestPracticesOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var bestPractices = await _service.GetCodingAgentBestPracticesAsync(cancellationToken);
            SetResult(context, new CodingAgentBestPracticesGetCommandResult(bestPractices));
        }
        catch (Exception ex)
        {
            // Log only the operation name; never log option objects or raw exception payloads.
            _logger.LogError(ex, "Error in {Operation}", Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CodingAgentBestPracticesGetCommandResult(string BestPractices);
}
