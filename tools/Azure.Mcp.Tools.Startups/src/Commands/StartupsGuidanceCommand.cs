// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Startups.Commands.Guidance;

public sealed class StartupsGuidanceCommand(ILogger<StartupsGuidanceCommand> logger) : BaseCommand
{
    private const string CommandTitle = "Get Guidance from Microsoft for Startups";
    private readonly ILogger<StartupsGuidanceCommand> _logger = logger;

    public override string Name => "get";
    public override string Description => "Receive program guidance for building with Microsoft for Startups.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true
    };

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
                Name: "Code Generation Capabilities",
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
                Name: "Validation & Helpers",
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
                Prompt: "Use the startups deploy command: Deploy my static website to Azure using storage account myawesomesite in startups resource group",
                ExpectedResult: "Creates storage account in startups RG, enables static hosting, uploads files",
                Prerequisites: ["Azure CLI logged in with az login", "Files in output folder"]
            ),
            new SamplePrompt(
                Category: "React Development",
                Title: "Deploy React app with build",
                Prompt: "Use the startups deploy command: Deploy my React app to Azure using storage account startupapp2024 in startups resource group",
                ExpectedResult: "Runs npm install, npm build, deploys to Azure with SPA routing in startups RG",
                Prerequisites: ["React project with package.json", "Node.js installed", "Azure CLI logged in with az login"]
            ),
            new SamplePrompt(
                Category: "Archery Website Example",
                Title: "Deploy Archery website",
                Prompt: "Use the startups deploy command: Deploy the archery website to Azure using storage account archery2025 in startups resource group",
                ExpectedResult: "Deploys complete archery website with static hosting enabled",
                Prerequisites: ["Azure CLI logged in with az login", "archery-web folder with HTML/CSS/JS files"]
            ),
            new SamplePrompt(
                Category: "Static Website",
                Title: "Deploy any web project",
                Prompt: "Use the startups deploy command: Deploy this app to Azure using storage account mystartup2025 in startups resource group",
                ExpectedResult: "Automatically detects project type and deploys to Azure in startups RG",
                Prerequisites: ["Azure CLI logged in with az login"]
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
                Example: "✅ c./my-app/build  ❌ ./nonexistent/folder"
            ),
            new ValidationRule(
                Parameter: "azure-login",
                Rule: "Must be logged into Azure CLI",
                ErrorMessage: "Run 'az login' first to authenticate with Azure",
                Example: "✅ az login  ❌ az account show"
            )
        };

        var quickStart = new List<QuickStartStep>
        {
            new QuickStartStep(
                Order: 1,
                Title: "🌟 Apply to Microsoft for Startups",
                Description: "Visit the Microsoft for Startups website and submit your application",
                Commands: ["1. Go to https://startups.microsoft.com/", "2. Click 'Apply Now'", "3. Complete the application form"]
            ),
            new QuickStartStep(
                Order: 2,
                Title: "� Program Benefits Overview",
                Description: "Understand the key benefits available to you",
                Commands: [
                    "- Up to $150,000 in Azure credits",
                    "- Technical support and architecture guidance",
                    "- Go-to-market and sales enablement",
                    "- Access to Microsoft's partner ecosystem"
                ]
            ),
            new QuickStartStep(
                Order: 3,
                Title: "🚀 Getting Started",
                Description: "Initial steps after acceptance into the program",
                Commands: [
                    "1. Set up your Microsoft for Startups Founders Hub account",
                    "2. Activate your Azure benefits",
                    "3. Schedule your technical onboarding session",
                    "4. Explore available resources and learning paths"
                ]
            ),
            new QuickStartStep(
                Order: 4,
                Title: "🤝 Next Steps",
                Description: "Explore deployment options and additional resources",
                Commands: [
                    "- Use 'startups deploy' command when ready to deploy",
                    "- Join the Microsoft for Startups community",
                    "- Connect with other founders",
                    "- Access exclusive training and events"
                ]
            )
        };

        var info = new StartupsGuidanceInfo(
            Title: CommandTitle,
            Description: "Microsoft for Startups is a global program that helps startups succeed with access to technology, coaching, and support. Startups receive free Azure credits, technical resources, expert guidance, and opportunities to connect with Microsoft partners and customers. Learn more and apply at the website. This service helps startups quickly create, build, and deploy modern web applications to Azure.",
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

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false)]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.StartupsGuidanceCommandResult))]
internal partial class GuidanceJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
