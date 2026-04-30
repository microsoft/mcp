using Azure.Mcp.Tools.Monitor.Detectors;
using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Python greenfield projects (no existing telemetry).
/// Supports Django, Flask, FastAPI, GenAI apps, and other Python frameworks.
///
/// Based on Azure Monitor OpenTelemetry Distro for Python:
/// https://github.com/Azure/azure-sdk-for-python/tree/main/sdk/monitor/azure-monitor-opentelemetry
/// </summary>
public class PythonGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        // Python project, greenfield, with at least one recognized framework
        if (analysis.Language != Language.Python)
            return false;

        if (analysis.State != InstrumentationState.Greenfield)
            return false;

        // Check if we have at least one project with a known app type
        var knownAppTypes = new[] { AppType.Django, AppType.Flask, AppType.FastAPI, AppType.Falcon, AppType.Starlette, AppType.Console, AppType.GenAI };
        return analysis.Projects.Any(p => knownAppTypes.Contains(p.AppType));
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType != AppType.Unknown);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint;
        var projectDir = Path.GetDirectoryName(projectFile) ?? "";
        var appType = project.AppType;
        var dependencies = project.Dependencies;

        // Get description based on app type
        var description = appType switch
        {
            AppType.GenAI => "GenAI greenfield application. Azure Monitor OpenTelemetry Distro with GenAI instrumentations for tracing LLM calls, token usage, and model interactions.",
            AppType.Console => "Generic Python console/script application. Azure Monitor OpenTelemetry Distro provides basic telemetry. Add library-specific instrumentations for HTTP clients, databases, etc.",
            _ => $"{appType} greenfield application. Azure Monitor OpenTelemetry Distro provides automatic instrumentation for HTTP requests, database calls, and custom telemetry."
        };

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "azure-monitor-opentelemetry",
                description);

        // Step 1: Review educational materials
        builder.AddReviewEducationAction(
            "review-education",
            "Review educational materials before implementation",
            GetEducationResources(appType));

        // Step 2: Add azure-monitor-opentelemetry package
        builder.AddPackageAction(
            "add-monitor-package",
            "Add Azure Monitor OpenTelemetry package",
            "pip",
            "azure-monitor-opentelemetry",
            ">=1.8.3",
            projectFile,
            "review-education");

        // Step 2b: Add GenAI instrumentation packages if needed
        var lastPackageAction = "add-monitor-package";
        if (appType == AppType.GenAI)
        {
            var genaiPackages = GetGenAIInstrumentationPackages(dependencies);
            for (int i = 0; i < genaiPackages.Count; i++)
            {
                var pkg = genaiPackages[i];
                var actionId = $"add-genai-package-{i + 1}";
                builder.AddPackageAction(
                    actionId,
                    $"Add {pkg.DisplayName} instrumentation package",
                    "pip",
                    pkg.InstrumentationPackage,
                    "latest",
                    projectFile,
                    lastPackageAction);
                lastPackageAction = actionId;
            }
        }

        // Step 2c: Add Console app instrumentation packages if needed
        if (appType == AppType.Console)
        {
            var consolePackages = GetConsoleInstrumentationPackages(dependencies);
            for (int i = 0; i < consolePackages.Count; i++)
            {
                var pkg = consolePackages[i];
                var actionId = $"add-instrumentation-{i + 1}";
                builder.AddPackageAction(
                    actionId,
                    $"Add {pkg.DisplayName} instrumentation package",
                    "pip",
                    pkg.InstrumentationPackage,
                    "latest",
                    projectFile,
                    lastPackageAction);
                lastPackageAction = actionId;
            }
        }

        // Step 3: Add instrumentation code to entry point
        if (entryPoint != null)
        {
            builder.AddModifyCodeAction(
                "configure-opentelemetry",
                "Initialize Azure Monitor OpenTelemetry at application startup",
                entryPoint,
                GetInstrumentationCode(appType, dependencies),
                GetInsertLocation(appType),
                "azure.monitor.opentelemetry",
                lastPackageAction);
        }
        else
        {
            // If no entry point found, add a manual step
            builder.AddManualStepAction(
                "configure-opentelemetry",
                "Add Azure Monitor initialization to your application entry point",
                GetManualInstructions(appType),
                [
                    "https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python"
                ],
                lastPackageAction);
        }

        // Step 4: Configure connection string with full .env.example content
        builder.AddConfigAction(
            "add-env-config",
            "Create .env.example with Azure Monitor configuration",
            Path.Combine(projectDir, ".env.example"),
            "APPLICATIONINSIGHTS_CONNECTION_STRING",
            GetEnvFileContent(appType, dependencies),
            "APPLICATIONINSIGHTS_CONNECTION_STRING");

        // Step 5: Add PowerShell script for setting environment variables
        builder.AddConfigAction(
            "add-powershell-script",
            "Create PowerShell script for setting environment variables",
            Path.Combine(projectDir, "set-env.ps1"),
            "script",
            GetPowerShellScriptContent(appType, dependencies),
            null);

        return builder.Build();
    }

    /// <summary>
    /// Get education resources based on framework type.
    /// </summary>
    private static List<string> GetEducationResources(AppType appType)
    {
        var resources = new List<string>
        {
            LearningResources.ConceptsOpenTelemetryPipelinePython,
            LearningResources.ConceptsAzureMonitorPython
        };

        // Add framework-specific example
        var frameworkExample = appType switch
        {
            AppType.Django => LearningResources.ExampleDjangoSetup,
            AppType.Flask => LearningResources.ExampleFlaskSetup,
            AppType.FastAPI => LearningResources.ExampleFastApiSetup,
            AppType.Console => LearningResources.ExampleConsolePythonSetup,
            AppType.GenAI => LearningResources.ExampleGenAiSetup,
            _ => LearningResources.ExampleGenericPythonSetup
        };
        resources.Add(frameworkExample);

        return resources;
    }

    /// <summary>
    /// Get GenAI instrumentation packages based on detected dependencies.
    /// Returns the instrumentation packages for GenAI libraries found in the project.
    /// </summary>
    private static List<InstrumentationInfo> GetGenAIInstrumentationPackages(List<string> dependencies)
    {
        var genaiInstrumentations = PythonInstrumentationRegistry.GetByCategory("genai");
        var result = new List<InstrumentationInfo>();

        foreach (var dep in dependencies)
        {
            var normalized = PythonInstrumentationRegistry.NormalizePackageName(dep);
            var instrumentation = genaiInstrumentations
                .FirstOrDefault(g => PythonInstrumentationRegistry.NormalizePackageName(g.LibraryName) == normalized);

            if (instrumentation != null && !string.IsNullOrEmpty(instrumentation.InstrumentationPackage))
            {
                // Avoid duplicates (e.g., langchain-core and langchain-community use same instrumentation)
                if (!result.Any(r => r.InstrumentationPackage == instrumentation.InstrumentationPackage))
                {
                    result.Add(instrumentation);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get Console app instrumentation packages based on detected dependencies.
    /// Returns instrumentation packages for HTTP clients, databases, and other libraries.
    /// Always includes logging instrumentation since it's a built-in module.
    /// </summary>
    private static List<InstrumentationInfo> GetConsoleInstrumentationPackages(List<string> dependencies)
    {
        var result = new List<InstrumentationInfo>();
        var normalized = dependencies.Select(PythonInstrumentationRegistry.NormalizePackageName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Always add logging instrumentation for console apps (logging is built-in, so not in requirements.txt)
        var loggingInstrumentation = new InstrumentationInfo
        {
            LibraryName = "logging",
            DisplayName = "Logging",
            ModuleName = "logging",
            InstrumentationPackage = "opentelemetry-instrumentation-logging",
            InDistro = false,
            Category = "other"
        };
        result.Add(loggingInstrumentation);

        // Check for HTTP libraries
        var httpLibraries = new[] { "requests", "httpx", "urllib3", "urllib", "aiohttp" };
        foreach (var lib in httpLibraries)
        {
            if (normalized.Contains(lib))
            {
                var instrumentation = PythonInstrumentationRegistry.GetInstrumentation(lib);
                if (instrumentation != null && !string.IsNullOrEmpty(instrumentation.InstrumentationPackage))
                {
                    if (!result.Any(r => r.InstrumentationPackage == instrumentation.InstrumentationPackage))
                    {
                        result.Add(instrumentation);
                    }
                }
            }
        }

        // Check for database libraries
        var dbLibraries = new[] { "psycopg2", "psycopg2-binary", "pymongo", "redis", "pymysql", "mysql-connector-python", "sqlalchemy" };
        foreach (var lib in dbLibraries)
        {
            if (normalized.Contains(lib))
            {
                var instrumentation = PythonInstrumentationRegistry.GetInstrumentation(lib);
                if (instrumentation != null && !string.IsNullOrEmpty(instrumentation.InstrumentationPackage))
                {
                    if (!result.Any(r => r.InstrumentationPackage == instrumentation.InstrumentationPackage))
                    {
                        result.Add(instrumentation);
                    }
                }
            }
        }

        // Check for async libraries
        if (normalized.Contains("asyncio") || normalized.Contains("aiohttp"))
        {
            var asyncioInstrumentation = PythonInstrumentationRegistry.GetInstrumentation("asyncio");
            if (asyncioInstrumentation != null && !string.IsNullOrEmpty(asyncioInstrumentation.InstrumentationPackage))
            {
                if (!result.Any(r => r.InstrumentationPackage == asyncioInstrumentation.InstrumentationPackage))
                {
                    result.Add(asyncioInstrumentation);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get the instrumentation code snippet for the specific framework.
    /// </summary>
    private static string GetInstrumentationCode(AppType appType, List<string> dependencies)
    {
        // Basic instrumentation code that works for all frameworks
        // The distro auto-detects and instruments Django, Flask, FastAPI, etc.
        const string basicCode = @"from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor OpenTelemetry - must be called before importing app frameworks
configure_azure_monitor()
";

        return appType switch
        {
            AppType.Django => @"from azure.monitor.opentelemetry import configure_azure_monitor
    configure_azure_monitor()

",
            AppType.Flask => @"from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor OpenTelemetry - call before creating Flask app
configure_azure_monitor()

# Your Flask app code follows
# from flask import Flask
# app = Flask(__name__)
",
            AppType.FastAPI => @"from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor OpenTelemetry - call before creating FastAPI app
configure_azure_monitor()

# Your FastAPI app code follows
# from fastapi import FastAPI
# app = FastAPI()
",
            AppType.Console => @"from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

",
            AppType.GenAI => GetGenAIInstrumentationCode(dependencies),
            _ => basicCode
        };
    }

    /// <summary>
    /// Generate GenAI instrumentation code including library-specific instrumentors.
    /// </summary>
    private static string GetGenAIInstrumentationCode(List<string> dependencies)
    {
        var code = @"import logging

logging.basicConfig(level=logging.INFO)

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

";

        // Add instrumentor calls for detected GenAI libraries
        var genaiPackages = GetGenAIInstrumentationPackages(dependencies);
        if (genaiPackages.Any())
        {
            foreach (var pkg in genaiPackages)
            {
                var instrumentorClass = GetInstrumentorClassName(pkg.LibraryName);
                if (!string.IsNullOrEmpty(instrumentorClass))
                {
                    var moduleName = pkg.LibraryName.ToLower().Replace("-", "").Replace("_", "");
                    code += $"from opentelemetry.instrumentation.{moduleName} import {instrumentorClass}\n";
                    code += $"{instrumentorClass}().instrument()\n\n";
                }
            }
        }

        code += "logger = logging.getLogger(__name__)\n";
        return code;
    }

    /// <summary>
    /// Get the instrumentor class name for a GenAI library.
    /// </summary>
    private static string GetInstrumentorClassName(string libraryName)
    {
        return libraryName.ToLower() switch
        {
            "openai" => "OpenAIInstrumentor",
            "anthropic" => "AnthropicInstrumentor",
            "langchain" => "LangchainInstrumentor",
            "langchain-core" => "LangchainInstrumentor",
            "langchain-community" => "LangchainInstrumentor",
            "google-cloud-aiplatform" => "VertexAIInstrumentor",
            "google-genai" => "GoogleGenAIInstrumentor",
            "openai-agents" => "OpenAIAgentsInstrumentor",
            "weaviate-client" => "WeaviateInstrumentor",
            _ => ""
        };
    }

    /// <summary>
    /// Get the location hint for where to insert the instrumentation code.
    /// </summary>
    private static string GetInsertLocation(AppType appType)
    {
        return appType switch
        {
            AppType.Django => "os.environ.setdefault('DJANGO_SETTINGS_MODULE'",
            AppType.Flask => "At the top of the file, before Flask import and app creation",
            AppType.FastAPI => "At the top of the file, before FastAPI import and app creation",
            AppType.Console => "logger = logging.getLogger(__name__)",
            AppType.GenAI => "At the very top of the file, before importing GenAI libraries (OpenAI, LangChain, Anthropic, etc.)",
            _ => "At the very top of the file, before other imports"
        };
    }

    /// <summary>
    /// Get manual instructions when entry point cannot be determined.
    /// </summary>
    private static string GetManualInstructions(AppType appType)
    {
        var baseInstructions = @"Add the following code at the very top of your application entry point:

```python
from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor - must be called before importing frameworks
configure_azure_monitor()
```

";

        var frameworkSpecific = appType switch
        {
            AppType.Django => @"For Django applications (manage.py):
- Add this inside the main() function
- Must be AFTER os.environ.setdefault('DJANGO_SETTINGS_MODULE', ...) line
- Must be BEFORE Django imports (execute_from_command_line)

Alternatively, for wsgi.py/asgi.py:
- Add at the very top of the file before importing Django
- Must be BEFORE get_wsgi_application() or get_asgi_application()",

            AppType.Flask => @"For Flask applications:
- Add this to the top of your main application file (e.g., `app.py`)
- Must be before you create the Flask app instance",

            AppType.FastAPI => @"For FastAPI applications:
- Add this to the top of your main application file (e.g., `main.py`)
- Must be before you create the FastAPI app instance",

            AppType.GenAI => @"For GenAI applications (OpenAI, LangChain, Anthropic, etc.):
- Add this to the top of your main application file (e.g., `app.py`)
- Must be before you import any GenAI libraries (openai, langchain, anthropic)
- This enables tracing for LLM calls, token usage, and model interactions",

            AppType.Console => @"For Console/Script applications:
- Add this to the top of your main script file (e.g., `app.py`, `main.py`)
- For library-specific tracing (requests, httpx, psycopg2), add manual instrumentations
- Use OpenTelemetry APIs to create custom spans for your business logic",

            _ => @"Add this to your application's entry point file before any other imports."
        };

        return baseInstructions + frameworkSpecific;
    }

    /// <summary>
    /// Get the full .env.example file content matching config_generator.py logic.
    /// </summary>
    private static string GetEnvFileContent(AppType appType, List<string> dependencies)
    {
        var baseContent = @"# Azure Monitor OpenTelemetry Configuration

# Required: Application Insights connection string
# Get this from Azure Portal -> Application Insights -> Overview
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/;LiveEndpoint=https://your-region.livediagnostics.monitor.azure.com/
";

        // Add GenAI-specific API keys
        if (appType == AppType.GenAI)
        {
            var genaiSection = GetGenAIApiKeySection(dependencies);
            if (!string.IsNullOrEmpty(genaiSection))
            {
                baseContent += "\n" + genaiSection;
            }
        }

        baseContent += @"
# Optional: Service name (defaults to application name)
# OTEL_SERVICE_NAME=my-application

# Optional: Resource attributes
# OTEL_RESOURCE_ATTRIBUTES=deployment.environment=production,service.version=1.0.0

# Optional: Sampling rate (0.0 to 1.0, default is 1.0)
# OTEL_TRACES_SAMPLER=traceidratio
# OTEL_TRACES_SAMPLER_ARG=0.1

# Optional: Enable/disable specific instrumentations
# OTEL_PYTHON_DISABLED_INSTRUMENTATIONS=

# Optional: Log level for OpenTelemetry
# OTEL_LOG_LEVEL=info
";

        return baseContent;
    }

    /// <summary>
    /// Get the PowerShell script content for setting environment variables.
    /// </summary>
    private static string GetPowerShellScriptContent(AppType appType, List<string> dependencies)
    {
        var baseScript = @"# Set Azure Monitor OpenTelemetry environment variables
# Copy this file to set-env.local.ps1 and update with your values

$env:APPLICATIONINSIGHTS_CONNECTION_STRING = ""InstrumentationKey=YOUR-KEY;IngestionEndpoint=https://YOUR-REGION.in.applicationinsights.azure.com/""
";

        // Add GenAI-specific API keys
        if (appType == AppType.GenAI)
        {
            var genaiSection = GetGenAIApiKeyScriptSection(dependencies);
            if (!string.IsNullOrEmpty(genaiSection))
            {
                baseScript += "\n" + genaiSection;
            }
        }

        baseScript += @"
# Optional settings
# $env:OTEL_SERVICE_NAME = ""my-application""
# $env:OTEL_RESOURCE_ATTRIBUTES = ""deployment.environment=production""

Write-Host ""Environment variables set for current session"" -ForegroundColor Green
Write-Host ""Run your application now: python app.py"" -ForegroundColor Cyan
";

        return baseScript;
    }

    /// <summary>
    /// Get GenAI API key environment variable section for .env files.
    /// </summary>
    private static string GetGenAIApiKeySection(List<string> dependencies)
    {
        var normalized = dependencies.Select(PythonInstrumentationRegistry.NormalizePackageName).ToList();
        var sections = new List<string>();

        if (normalized.Contains("openai") || normalized.Contains("openai-agents"))
        {
            sections.Add(@"# Required: OpenAI API Key
# Get this from https://platform.openai.com/api-keys
OPENAI_API_KEY=sk-your-openai-api-key-here");
        }

        if (normalized.Contains("anthropic"))
        {
            sections.Add(@"# Required: Anthropic API Key
# Get this from https://console.anthropic.com/
ANTHROPIC_API_KEY=sk-ant-your-anthropic-api-key-here");
        }

        if (normalized.Contains("langchain") || normalized.Contains("langchain-core") || normalized.Contains("langchain-community"))
        {
            // LangChain often uses OpenAI, but could use other providers
            if (!normalized.Contains("openai"))
            {
                sections.Add(@"# Required: LangChain LLM API Key (e.g., OpenAI)
# LangChain supports multiple providers - configure the one you're using
OPENAI_API_KEY=sk-your-api-key-here");
            }
        }

        if (normalized.Contains("google-cloud-aiplatform") || normalized.Contains("google-genai"))
        {
            sections.Add(@"# Required: Google Cloud credentials
# Set up using: gcloud auth application-default login
# Or set GOOGLE_APPLICATION_CREDENTIALS=path/to/service-account.json");
        }

        return sections.Count > 0 ? "\n" + string.Join("\n\n", sections) : string.Empty;
    }

    /// <summary>
    /// Get GenAI API key environment variable section for PowerShell scripts.
    /// </summary>
    private static string GetGenAIApiKeyScriptSection(List<string> dependencies)
    {
        var normalized = dependencies.Select(PythonInstrumentationRegistry.NormalizePackageName).ToList();
        var sections = new List<string>();

        if (normalized.Contains("openai") || normalized.Contains("openai-agents"))
        {
            sections.Add(@"# Required: OpenAI API Key
$env:OPENAI_API_KEY = ""sk-your-openai-api-key-here""");
        }

        if (normalized.Contains("anthropic"))
        {
            sections.Add(@"# Required: Anthropic API Key
$env:ANTHROPIC_API_KEY = ""sk-ant-your-anthropic-api-key-here""");
        }

        if (normalized.Contains("langchain") || normalized.Contains("langchain-core") || normalized.Contains("langchain-community"))
        {
            if (!normalized.Contains("openai"))
            {
                sections.Add(@"# Required: LangChain LLM API Key
$env:OPENAI_API_KEY = ""sk-your-api-key-here""");
            }
        }

        if (normalized.Contains("google-cloud-aiplatform") || normalized.Contains("google-genai"))
        {
            sections.Add(@"# Required: Google Cloud credentials
# $env:GOOGLE_APPLICATION_CREDENTIALS = ""path/to/service-account.json""");
        }

        return sections.Count > 0 ? string.Join("\n", sections) : string.Empty;
    }
}
