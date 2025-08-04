// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Commands;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Commands.Guidance;

public sealed class StartupsGuidanceCommand(ILogger<StartupsGuidanceCommand> logger) : BaseCommand
{
    private const string CommandTitle = "Get Guidance from Microsoft for Startups";
    private readonly ILogger<StartupsGuidanceCommand> _logger = logger;
    
    public override string Name => "get";
    public override string Description => "Receive program guidance for building with Microsoft for Startups.";
    public override string Title => CommandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var capabilities = new List<CapabilitySection>
        {
            new CapabilitySection(
                Name: "Deployment Capabilities",
                Description: "Deploy your applications to Azure",
                Items: new List<Capability>
                {
                    new Capability(
                        Name: "Static Website Deployment",
                        Description: "Deploy HTML, CSS, JS files to Azure Static Website hosting",
                        RequiredParameters: ["tenant", "subscription", "storage-account", "resource-group", "source-path"],
                        OptionalParameters: ["overwrite"],
                        SampleUsage: "Deploy the 'output' folder to storage account 'mywebsite'",
                        IsImplemented: true
                    ),
                    new Capability(
                        Name: "React App Deployment", 
                        Description: "Build and deploy React applications with SPA routing",
                        RequiredParameters: ["tenant", "subscription", "storage-account", "resource-group", "react-project"],
                        OptionalParameters: ["build", "build-path", "overwrite"],
                        SampleUsage: "Deploy React app from 'my-react-app' folder with automatic build",
                        IsImplemented: true
                    )
                }
            ),
            new CapabilitySection(
                Name: "🛠️ Code Generation Capabilities",
                Description: "Generate and scaffold new applications",
                Items: new List<Capability>
                {
                    new Capability(
                        Name: "React App Creation",
                        Description: "Generate a complete React application",
                        RequiredParameters: ["project-path", "app-name"],
                        OptionalParameters: ["typescript", "router", "tailwind", "additional-packages"],
                        SampleUsage: "Create a React app called 'my-startup' with TypeScript",
                        IsImplemented: false // Set to true when code generation service
                    ),
                    new Capability(
                        Name: "Static Website Creation",
                        Description: "Generate a static website with interactive design",
                        RequiredParameters: ["project-path", "site-name"],
                        OptionalParameters: ["theme", "bootstrap", "jquery", "title", "description"],
                        SampleUsage: "Create a landing page called 'awesome-startup'",
                        IsImplemented: false
                    )
                }
            ),
            new CapabilitySection(
                Name: "📋 Validation & Helpers",
                Description: "Built-in validation and helper functions",
                Items: new List<Capability>
                {
                    new Capability(
                        Name: "Storage Account Name Validation",
                        Description: "Validates Azure storage account naming rules and global availability",
                        RequiredParameters: ["storage-account-name"],
                        OptionalParameters: [],
                        SampleUsage: "Automatically validates 'mystartupapp2024' is available globally",
                        IsImplemented: true
                    ),
                    new Capability(
                        Name: "Project Structure Validation",
                        Description: "Validates React projects and source directories",
                        RequiredParameters: ["project-path"],
                        OptionalParameters: [],
                        SampleUsage: "Checks for package.json and build output before deployment",
                        IsImplemented: true
                    )
                }
            )
        };

        var samplePrompts = new List<SamplePrompt>
        {
            new SamplePrompt(
                Category: "Quick Deployment",
                Title: "Deploy existing static website",
                Prompt: "Deploy the 'output' folder to a storage account called 'myawesomesite'",
                ExpectedResult: "Creates storage account, enables static hosting, uploads files",
                Prerequisites: ["Azure CLI logged in", "Files in output folder"]
            ),
            new SamplePrompt(
                Category: "React Development",
                Title: "Deploy React app with build",
                Prompt: "Deploy my React app from 'my-startup-app' folder to storage account 'startupapp2024'",
                ExpectedResult: "Runs npm install, npm build, deploys to Azure with SPA routing",
                Prerequisites: ["React project with package.json", "Node.js installed"]
            ),
            new SamplePrompt(
                Category: "Code Generation",
                Title: "Create and deploy React app",
                Prompt: "Create a new React app called 'awesome-startup' with TypeScript and deploy it to 'awesomestartup'",
                ExpectedResult: "Generates React app, builds it, and deploys to Azure",
                Prerequisites: ["Node.js installed", "Azure CLI logged in"]
            ),
            new SamplePrompt(
                Category: "Static Website",
                Title: "Create landing page",
                Prompt: "Create a landing page for 'ArcheryStartup' and deploy to 'techstartuplanding'",
                ExpectedResult: "Generates professional landing page and deploys to Azure",
                Prerequisites: ["Azure CLI logged in"]
            )
        };

        var validationRules = new List<ValidationRule>
        {
            new ValidationRule(
                Parameter: "storage-account",
                Rule: "3-24 characters, lowercase letters and numbers only, globally unique",
                ErrorMessage: "Storage account name must be between 3-24 characters long and only contain letters and numbers",
                Example: "✅ mystartup2024  ❌ MyStartup-2024"
            ),
            new ValidationRule(
                Parameter: "source-path",
                Rule: "Must be an existing directory with files",
                ErrorMessage: "Source directory does not exist or is empty",
                Example: "✅ c:\\projects\\my-app\\build  ❌ c:\\nonexistent\\folder"
            ),
            new ValidationRule(
                Parameter: "react-project",
                Rule: "Must contain package.json file",
                ErrorMessage: "Directory must be a valid React project with package.json",
                Example: "✅ c:\\projects\\my-react-app  ❌ c:\\projects\\static-html"
            )
        };

        var quickStart = new List<QuickStartStep>
        {
            new QuickStartStep(
                Order: 1,
                Title: "Authentication Setup",
                Description: "Ensure you're logged into Azure CLI",
                Commands: ["az login", "az account show"]
            ),
            new QuickStartStep(
                Order: 2,
                Title: "📁 Prepare Your Project",
                Description: "Have your website files ready for deployment",
                Commands: [
                    "For static sites: Put files in a folder",
                    "For React: Ensure package.json exists",
                    "Run 'npm run build' if needed"
                ]
            ),
            new QuickStartStep(
                Order: 3,
                Title: "Deploy Your First App",
                Description: "Try deploying with a simple command",
                Commands: [
                    "Deploy 'output' folder to storage account 'testsite123'",
                    "Deploy React app from 'my-app' to storage account 'myreactapp'"
                ]
            ),
            new QuickStartStep(
                Order: 4,
                Title: "🌐 Access Your Website",
                Description: "Your site will be available at the Azure static website URL",
                Commands: [
                    "URL format: https://{storage-account}.z13.web.core.windows.net",
                    "Example: https://myreactapp.z13.web.core.windows.net"
                ]
            )
        };

        var info = new StartupsGuidanceInfo(
            Title: CommandTitle,
            Description: "Your comprehensive guide to building and deploying web applications with Azure. This service helps startups quickly create, build, and deploy modern web applications to Azure Static Website hosting.",
            Link: "https://startups.microsoft.com/",
            Capabilities: capabilities,
            SamplePrompts: samplePrompts,
            ValidationRules: validationRules,
            QuickStart: quickStart
        );

        context.Response.Results = ResponseResult.Create<StartupsGuidanceCommandResult>(
            new StartupsGuidanceCommandResult(info),
            GuidanceJsonContext.Default.StartupsGuidanceCommandResult);
        
        return Task.FromResult(context.Response);
    }

    public record StartupsGuidanceInfo(
        string Title,
        string Description,
        string Link,
        List<CapabilitySection> Capabilities,
        List<SamplePrompt> SamplePrompts,
        List<ValidationRule> ValidationRules,
        List<QuickStartStep> QuickStart
    );

    public record CapabilitySection(
        string Name,
        string Description,
        List<Capability> Items,
        bool IsAvailable = true
    );

    public record Capability(
        string Name,
        string Description,
        List<string> RequiredParameters,
        List<string> OptionalParameters,
        string SampleUsage,
        bool IsImplemented = true
    );

    public record SamplePrompt(
        string Category,
        string Title,
        string Prompt,
        string ExpectedResult,
        List<string> Prerequisites
    );

    public record ValidationRule(
        string Parameter,
        string Rule,
        string ErrorMessage,
        string Example
    );

    public record QuickStartStep(
        int Order,
        string Title,
        string Description,
        List<string> Commands,
        bool IsOptional = false
    );

    public record StartupsGuidanceCommandResult(StartupsGuidanceInfo Info);
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.StartupsGuidanceCommandResult))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.StartupsGuidanceInfo))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.CapabilitySection))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.Capability))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.SamplePrompt))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.ValidationRule))]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.QuickStartStep))]
internal partial class GuidanceJsonContext : System.Text.Json.Serialization.JsonSerializerContext;