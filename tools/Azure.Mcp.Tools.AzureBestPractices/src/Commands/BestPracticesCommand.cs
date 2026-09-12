// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using Azure.Mcp.Tools.AzureBestPractices.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands;

[CommandMetadata(
    Id = "ff12e8fb-f7ce-446a-884b-996dac118b83",
    Name = "get",
    Title = "Get Azure Best Practices",
    Description = """
        This tool returns a list of best practices for code generation, operations and deployment
        when working with Azure services. It should be called for any code generation, deployment or
        operations involving Azure, Azure Functions, Azure Kubernetes Service (AKS), Azure Container
        Apps (ACA), Bicep, Terraform, Azure Cache, Redis, CosmosDB, Entra, Azure Active Directory,
        Azure App Services, or any other Azure technology or programming language. Only call this function
        when you are confident the user is discussing Azure. If this tool needs to be categorized,
        it belongs to the Azure Best Practices category.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BestPracticesCommand(ILogger<BestPracticesCommand> logger)
    : BaseCommand<BestPracticesOptions, BestPracticesCommand.BestPracticesCommandResult>
{
    private readonly ILogger<BestPracticesCommand> _logger = logger;
    private static readonly ConcurrentDictionary<string, string> s_bestPracticesCache = [];

    public override void ValidateOptions(BestPracticesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Resource is BestPracticesResource.StaticWebApp or BestPracticesResource.CodingAgent &&
            options.Action != BestPracticesAction.All)
        {
            validationResult.Errors.Add($"The '{GetResourceValue(options.Resource)}' resource only supports 'all' action.");
        }
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, BestPracticesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resourceFileName = GetResourceFileName(options.Resource, options.Action);
            var bestPractices = GetBestPracticesText(resourceFileName);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(
                new([bestPractices]),
                AzureBestPracticesJsonContext.Default.BestPracticesCommandResult);
            context.Response.Message = string.Empty;

            context.Activity?.AddTag("BestPractices_Resource", options.Resource);
            context.Activity?.AddTag("BestPractices_Action", options.Action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best practices for Resource: {Resource}, Action: {Action}",
                options.Resource, options.Action);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private static string GetResourceFileName(BestPracticesResource resource, BestPracticesAction action)
    {
        return (resource, action) switch
        {
            (BestPracticesResource.General, BestPracticesAction.CodeGeneration) => "azure-general-codegen-best-practices.txt",
            (BestPracticesResource.General, BestPracticesAction.Deployment) => "azure-general-deployment-best-practices.txt",
            (BestPracticesResource.General, BestPracticesAction.All) => "azure-general-codegen-best-practices.txt,azure-general-deployment-best-practices.txt",
            (BestPracticesResource.AzureFunctions, BestPracticesAction.CodeGeneration) => "azure-functions-codegen-best-practices.txt",
            (BestPracticesResource.AzureFunctions, BestPracticesAction.Deployment) => "azure-functions-deployment-best-practices.txt",
            (BestPracticesResource.AzureFunctions, BestPracticesAction.All) => "azure-functions-codegen-best-practices.txt,azure-functions-deployment-best-practices.txt",
            (BestPracticesResource.StaticWebApp, BestPracticesAction.All) => "azure-swa-best-practices.txt",
            (BestPracticesResource.CodingAgent, BestPracticesAction.All) => "azure-coding-agent-best-practices.txt",
            _ => throw new ArgumentException($"Invalid combination of resource '{resource}' and action '{action}'")
        };
    }

    private static string GetResourceValue(BestPracticesResource resource) => resource switch
    {
        BestPracticesResource.General => "general",
        BestPracticesResource.AzureFunctions => "azurefunctions",
        BestPracticesResource.StaticWebApp => "static-web-app",
        BestPracticesResource.CodingAgent => "coding-agent",
        _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
    };

    private static string GetBestPracticesText(string resourceFileName)
    {
        if (string.IsNullOrEmpty(resourceFileName))
        {
            throw new ArgumentException("Resource file name cannot be null or empty.", nameof(resourceFileName));
        }

        if (!s_bestPracticesCache.TryGetValue(resourceFileName, out string? bestPractices))
        {
            bestPractices = LoadBestPracticesText(resourceFileName);
            s_bestPracticesCache[resourceFileName] = bestPractices;
        }
        return bestPractices;
    }

    private static string LoadBestPracticesText(string resourceFileName)
    {
        Assembly assembly = typeof(BestPracticesCommand).Assembly;

        // Handle multiple files separated by comma
        if (resourceFileName.Contains(','))
        {
            var fileNames = resourceFileName.Split(',');
            var combinedContent = new StringBuilder();

            foreach (var fileName in fileNames)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
                }

                string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, fileName.Trim());
                string content = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);

                if (combinedContent.Length > 0)
                {
                    combinedContent.Append("\n\n");
                }
                combinedContent.Append(content);
            }

            return combinedContent.ToString();
        }
        else
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceFileName);
            return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
        }
    }

    public sealed record BestPracticesCommandResult(List<string> BestPractices);
}
