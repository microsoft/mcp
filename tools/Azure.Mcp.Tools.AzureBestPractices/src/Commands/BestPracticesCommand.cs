// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Reflection;
using System.Text;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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
        @"This tool returns a list of best practices for code generation, operations and deployment
        when working with Azure services. It should be called for any code generation, deployment or
        operations involving Azure, Azure Functions, Azure Kubernetes Service (AKS), Azure Container
        Apps (ACA), Bicep, Terraform, Azure Cache, Redis, CosmosDB, Entra, Azure Active Directory,
        Azure App Services, or any other Azure technology or programming language. Only call this function
        when you are confident the user is discussing Azure. If this tool needs to be categorized,
        it belongs to the Azure Best Practices category.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(_resourceOption);
        command.Options.Add(_actionOption);
    }

    private BestPracticesOptions BindOptions(ParseResult parseResult)
    {
        return new BestPracticesOptions
        {
            Resource = parseResult.CommandResult.GetValue(BestPracticesOptionDefinitions.Resource),
            Action = parseResult.CommandResult.GetValue(BestPracticesOptionDefinitions.Action)
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            if (string.IsNullOrEmpty(options.Resource) || string.IsNullOrEmpty(options.Action))
            {
                context.Response.Status = 400;
                context.Response.Message = "Both resource and action parameters are required.";
                return Task.FromResult(context.Response);
            }

            var resourceFileName = GetResourceFileName(options.Resource, options.Action);
            var bestPractices = GetBestPracticesText(resourceFileName);

            context.Response.Status = 200;
            context.Response.Results = ResponseResult.Create(new List<string> { bestPractices }, AzureBestPracticesJsonContext.Default.ListString);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error getting best practices for Resource: {Resource}, Action: {Action}",
                options.Resource, options.Action);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = new ValidationResult { IsValid = true };

        commandResult.TryGetValue(BestPracticesOptionDefinitions.Resource, out string? resource);
        commandResult.TryGetValue(BestPracticesOptionDefinitions.Action, out string? action);

        if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action))
        {
            result.IsValid = false;
            result.ErrorMessage = "Both resource and action parameters are required.";
        }
        else
        {
            bool validResource = resource == "general" || resource == "azurefunctions" || resource == "static-web-app";
            bool validAction = action == "all" || action == "code-generation" || action == "deployment";

            if (!validResource)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid resource. Must be 'general', 'azurefunctions', or 'static-web-app'.";
            }
            else if (!validAction)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid action. Must be 'all', 'code-generation' or 'deployment'.";
            }
            else if (resource == "static-web-app" && action != "all")
            {
                result.IsValid = false;
                result.ErrorMessage = "The 'static-web-app' resource only supports 'all' action.";
            }
        }

        if (!result.IsValid && commandResponse != null)
        {
            commandResponse.Status = 400;
            commandResponse.Message = result.ErrorMessage!;
        }

        return result;
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
