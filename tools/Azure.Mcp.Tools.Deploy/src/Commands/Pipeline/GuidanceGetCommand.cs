// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Deploy.Commands.Pipeline;

[CommandMetadata(
    Id = "8aec84f9-e884-4119-a386-53b7cfbe9e00",
    Name = "get",
    Title = "Get Azure Deployment CI/CD Pipeline Guidance",
    Description = "Provides the recommended Azure-specific rules for generating CI/CD pipeline files (GitHub Actions or Azure DevOps) for deploying to Azure. Call this tool before writing pipeline/workflow YAML when the user asks to set up CI/CD pipelines or workflows. It returns current authentication patterns (e.g., OIDC with managed identity), multi-environment configuration, and deployment constraints that vary by platform and are not covered by general best practices. Determine the pipeline platform (github-actions or azure-devops) and deployment scope (deploy-only or provision-and-deploy) from project context before calling. Handles both azd-based and Azure CLI-based deployments.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GuidanceGetCommand() : BaseCommand<GuidanceGetOptions, string>()
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GuidanceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = PipelineGenerationUtil.GeneratePipelineGuidelines(options);

            context.Response.Message = result;
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }
        return context.Response;
    }
}
