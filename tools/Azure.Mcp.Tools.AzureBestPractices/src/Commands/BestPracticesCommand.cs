// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.AzureBestPractices.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands;

public sealed class BestPracticesCommand(ILogger<BestPracticesCommand> logger) : BaseCommand
{
    private const string CommandTitle = "Get Azure Best Practices";
    private readonly ILogger<BestPracticesCommand> _logger = logger;
    private static readonly Dictionary<string, string> s_bestPracticesCache = new();

    private readonly Option<string> _resourceOption = BestPracticesOptionDefinitions.Resource;
    private readonly Option<string> _actionOption = BestPracticesOptionDefinitions.Action;

    public override string Name => "get";

    public override string Description =>
        @"Azure best practices – This tool returns secure, production-grade guidance and recommendations 
        for cloud architecture, security, performance, and cost optimization when working with Azure services. 
        Use it for Azure resource configuration, security hardening, deployment patterns, monitoring setup, 
        and before generating Azure SDK code or infrastructure templates (Bicep, Terraform, ARM). 
        It should be called for code generation, deployment, or operations involving Azure technologies such as 
        Azure Functions, Azure Kubernetes Service (AKS), Azure Container Apps (ACA), Azure Cache for Redis, 
        Cosmos DB, Entra ID (Azure Active Directory), Azure App Services, and other Azure services. 
        Do not use this tool for specific troubleshooting, real-time monitoring, or direct resource management 
        operations – it focuses on architectural and security best practices rather than executing actions. 
        If this tool needs to be categorized, it belongs to the Azure Best Practices category.";


    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        command.AddOption(_resourceOption);
        command.AddOption(_actionOption);
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var validationResult = Validate(parseResult.CommandResult, context.Response);
            if (!validationResult.IsValid)
            {
                return Task.FromResult(context.Response);
            }

            var resource = parseResult.GetValueForOption(_resourceOption);
            var action = parseResult.GetValueForOption(_actionOption);

            var resourceFileName = GetResourceFileName(resource!, action!);
            var bestPractices = GetBestPracticesText(resourceFileName);

            context.Response.Status = 200;
            context.Response.Results = ResponseResult.Create(new List<string> { bestPractices }, AzureBestPracticesJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best practices for Resource: {Resource}, Action: {Action}",
                parseResult.GetValueForOption(_resourceOption), parseResult.GetValueForOption(_actionOption));
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var validationResult = new ValidationResult { IsValid = true };

        var resource = commandResult.GetValueForOption(BestPracticesOptionDefinitions.Resource);
        var action = commandResult.GetValueForOption(BestPracticesOptionDefinitions.Action);

        if (string.IsNullOrEmpty(resource) || string.IsNullOrEmpty(action))
        {
            validationResult.IsValid = false;
            validationResult.ErrorMessage = "Both resource and action parameters are required.";
        }
        else if (resource != "general" && resource != "azurefunctions" && resource != "static-web-app")
        {
            validationResult.IsValid = false;
            validationResult.ErrorMessage = "Invalid resource. Must be 'general', 'azurefunctions', or 'static-web-app'.";
        }
        else if (action != "all" && action != "code-generation" && action != "deployment")
        {
            validationResult.IsValid = false;
            validationResult.ErrorMessage = "Invalid action. Must be 'all', 'code-generation' or 'deployment'.";
        }
        else if (resource == "static-web-app" && action != "all")
        {
            validationResult.IsValid = false;
            validationResult.ErrorMessage = "The 'static-web-app' resource only supports 'all' action.";
        }

        if (!validationResult.IsValid && commandResponse != null)
        {
            commandResponse.Status = 400;
            commandResponse.Message = validationResult.ErrorMessage!;
        }

        return validationResult;
    }

    private static string GetResourceFileName(string resource, string action)
    {
        return (resource, action) switch
        {
            ("general", "code-generation") => "azure-general-codegen-best-practices.txt",
            ("general", "deployment") => "azure-general-deployment-best-practices.txt",
            ("general", "all") => "azure-general-codegen-best-practices.txt,azure-general-deployment-best-practices.txt",
            ("azurefunctions", "code-generation") => "azure-functions-codegen-best-practices.txt",
            ("azurefunctions", "deployment") => "azure-functions-deployment-best-practices.txt",
            ("azurefunctions", "all") => "azure-functions-codegen-best-practices.txt,azure-functions-deployment-best-practices.txt",
            ("static-web-app", "all") => "azure-swa-best-practices.txt",
            _ => throw new ArgumentException($"Invalid combination of resource '{resource}' and action '{action}'")
        };
    }

    private string GetBestPracticesText(string resourceFileName)
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

    private string LoadBestPracticesText(string resourceFileName)
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
}
