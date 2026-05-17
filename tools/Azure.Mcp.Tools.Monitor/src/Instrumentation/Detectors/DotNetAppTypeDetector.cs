using System.Xml.Linq;
using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class DotNetAppTypeDetector : IAppTypeDetector
{
    public Language SupportedLanguage => Language.DotNet;

    public List<ProjectInfo> DetectProjects(string workspacePath)
    {
        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
        var projects = new List<ProjectInfo>();

        foreach (var csproj in csprojFiles)
        {
            var project = AnalyzeProject(csproj);
            projects.Add(project);
        }

        return projects;
    }

    private ProjectInfo AnalyzeProject(string csprojPath)
    {
        var appType = AppType.Unknown;
        string? entryPoint = null;
        var hostingPattern = HostingPattern.Unknown;

        try
        {
            var doc = XDocument.Load(csprojPath);
            var sdk = doc.Root?.Attribute("Sdk")?.Value ?? "";

            // Check if this is a legacy (non-SDK-style) project
            if (string.IsNullOrEmpty(sdk))
            {
                return DetectLegacyProjectType(csprojPath, doc);
            }

            // Determine app type from SDK
            appType = sdk switch
            {
                "Microsoft.NET.Sdk.Web" => AppType.AspNetCore,
                "Microsoft.NET.Sdk.Worker" => AppType.Worker,
                "Microsoft.NET.Sdk" => DetermineFromOutputType(doc),
                _ => AppType.Unknown
            };

            // Check for Azure Functions
            if (HasPackageReference(doc, "Microsoft.Azure.Functions.Worker") ||
                HasPackageReference(doc, "Microsoft.NET.Sdk.Functions"))
            {
                appType = AppType.AzureFunctions;
            }

            // Find entry point (only for non-libraries)
            if (appType != AppType.Library)
            {
                var projectDir = Path.GetDirectoryName(csprojPath)!;
                entryPoint = FindEntryPoint(projectDir);

                // Detect hosting pattern from entry point content
                hostingPattern = DetectHostingPattern(entryPoint);
            }
        }
        catch
        {
            // If we can't parse, return unknown
        }

        return new ProjectInfo
        {
            ProjectFile = csprojPath,
            EntryPoint = entryPoint,
            AppType = appType,
            HostingPattern = hostingPattern
        };
    }

    private AppType DetermineFromOutputType(XDocument doc)
    {
        var outputType = doc.Descendants("OutputType").FirstOrDefault()?.Value;
        return outputType?.ToLowerInvariant() switch
        {
            "exe" => AppType.Console,
            "library" => AppType.Library,
            _ => AppType.Console // Default for SDK-style projects
        };
    }

    private bool HasPackageReference(XDocument doc, string packageName)
    {
        return doc.Descendants("PackageReference")
            .Any(pr => string.Equals(pr.Attribute("Include")?.Value, packageName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Detects the hosting pattern by reading the entry point file content.
    /// Supports both Host.CreateDefaultBuilder/Host.CreateApplicationBuilder (generic host)
    /// and WebApplication.CreateBuilder (minimal API) patterns.
    /// </summary>
    private HostingPattern DetectHostingPattern(string? entryPointPath)
    {
        if (string.IsNullOrEmpty(entryPointPath) || !File.Exists(entryPointPath))
            return HostingPattern.Unknown;

        try
        {
            var content = File.ReadAllText(entryPointPath);

            // Check for ASP.NET Core minimal API pattern
            if (content.Contains("WebApplication.CreateBuilder"))
                return HostingPattern.MinimalApi;

            // Check for generic host patterns (Worker Service, console apps with hosting)
            if (content.Contains("Host.CreateDefaultBuilder") ||
                content.Contains("Host.CreateApplicationBuilder") ||
                content.Contains("HostApplicationBuilder"))
                return HostingPattern.GenericHost;

            return HostingPattern.Unknown;
        }
        catch
        {
            return HostingPattern.Unknown;
        }
    }

    private string? FindEntryPoint(string projectDir)
    {
        // Look for Program.cs first
        var programCs = Path.Combine(projectDir, "Program.cs");
        if (File.Exists(programCs))
            return programCs;

        // Look for Startup.cs (older pattern)
        var startupCs = Path.Combine(projectDir, "Startup.cs");
        if (File.Exists(startupCs))
            return startupCs;

        return null;
    }

    private ProjectInfo DetectLegacyProjectType(string csprojPath, XDocument doc)
    {
        var projectDir = Path.GetDirectoryName(csprojPath);
        if (projectDir == null)
        {
            throw new ArgumentException("The csprojPath must not be a root directory or null.", nameof(csprojPath));
        }
        var appType = AppType.Unknown;
        string? entryPoint = null;

        // Check for WCF
        if (IsWcfProject(projectDir, doc))
        {
            appType = AppType.WcfService;
            entryPoint = FindWcfServiceFile(projectDir);
        }
        // Check for OWIN
        else if (IsOwinProject(projectDir, doc))
        {
            appType = AppType.OwinApp;
            entryPoint = Path.Combine(projectDir, "Startup.cs");
        }
        // Check for classic ASP.NET
        else if (IsClassicAspNet(projectDir, doc))
        {
            // Differentiate between MVC and WebForms
            if (HasMvcIndicators(projectDir, doc))
            {
                appType = AppType.AspNetMvc;
                entryPoint = FindGlobalAsax(projectDir);
            }
            else if (HasWebFormsIndicators(projectDir))
            {
                appType = AppType.AspNetWebForms;
                entryPoint = FindGlobalAsax(projectDir);
            }
            else
            {
                appType = AppType.AspNetClassic;
                entryPoint = FindGlobalAsax(projectDir);
            }
        }
        // Legacy console or library
        else
        {
            var outputType = doc.Descendants("OutputType").FirstOrDefault()?.Value;
            appType = outputType?.ToLowerInvariant() switch
            {
                "exe" => AppType.Console,
                "winexe" => AppType.Console,
                "library" => AppType.Library,
                _ => AppType.Console
            };
            entryPoint = FindEntryPoint(projectDir);
        }

        return new ProjectInfo
        {
            ProjectFile = csprojPath,
            EntryPoint = entryPoint,
            AppType = appType
        };
    }

    private bool IsWcfProject(string projectDir, XDocument doc)
    {
        // Check for .svc files
        if (Directory.GetFiles(projectDir, "*.svc", SearchOption.AllDirectories).Any())
            return true;

        // Check for System.ServiceModel reference
        if (HasReference(doc, "System.ServiceModel"))
            return true;

        // Check for WCF package in packages.config
        var packagesConfig = Path.Combine(projectDir, "packages.config");
        if (File.Exists(packagesConfig))
        {
            try
            {
                var packagesDoc = XDocument.Load(packagesConfig);
                if (packagesDoc.Descendants("package")
                    .Any(p => p.Attribute("id")?.Value?.Contains("System.ServiceModel") == true))
                    return true;
            }
            catch (IOException)
            {
                // Log or handle IO errors
            }
            catch
            {
                // Handle other exceptions
            }
        }

        // Scan source files for WCF attributes
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
            .Take(50); // Limit scan for performance

        foreach (var file in csFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                if (content.Contains("ServiceContract") ||
                    content.Contains("OperationContract") ||
                    content.Contains("ServiceBehavior") ||
                    content.Contains("using System.ServiceModel"))
                {
                    return true;
                }
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        return false;
    }

    private bool IsOwinProject(string projectDir, XDocument doc)
    {
        // Check for OWIN packages in packages.config
        var packagesConfig = Path.Combine(projectDir, "packages.config");
        if (File.Exists(packagesConfig))
        {
            try
            {
                var packagesDoc = XDocument.Load(packagesConfig);
                if (packagesDoc.Descendants("package")
                    .Any(p => p.Attribute("id")?.Value?.Contains("Microsoft.Owin") == true ||
                             p.Attribute("id")?.Value == "Owin"))
                    return true;
            }
            catch (IOException)
            {
                // Log or handle IO errors
            }
            catch (Exception)
            {
                // Log or handle other exceptions
            }
        }

        // Check for OWIN references
        if (HasReference(doc, "Microsoft.Owin") || HasReference(doc, "Owin"))
            return true;

        // Look for Startup.cs with OWIN patterns
        var startupCs = Path.Combine(projectDir, "Startup.cs");
        if (File.Exists(startupCs))
        {
            try
            {
                var content = File.ReadAllText(startupCs);
                if (content.Contains("IAppBuilder") ||
                    content.Contains("using Owin") ||
                    content.Contains("using Microsoft.Owin"))
                {
                    return true;
                }
            }
            catch { }
        }

        // Scan source files for OWIN patterns
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
            .Take(50);

        foreach (var file in csFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                if (content.Contains("IAppBuilder") ||
                    content.Contains("IOwinContext") ||
                    content.Contains("OwinContext"))
                {
                    return true;
                }
            }
            catch { }
        }

        return false;
    }

    private bool IsClassicAspNet(string projectDir, XDocument doc)
    {
        // Check for Web.config
        var webConfig = Path.Combine(projectDir, "Web.config");
        if (!File.Exists(webConfig))
            return false;

        try
        {
            var webConfigDoc = XDocument.Load(webConfig);
            // Check for system.web section
            if (webConfigDoc.Descendants("system.web").Any())
                return true;
        }
        catch (IOException)
        {
            // Log or handle IOException if needed
        }
        catch (Exception)
        {
            // Log or handle any other exceptions if needed
        }

        // Check for System.Web reference
        if (HasReference(doc, "System.Web"))
            return true;

        // Check for ASP.NET packages
        var packagesConfig = Path.Combine(projectDir, "packages.config");
        if (File.Exists(packagesConfig))
        {
            var packagesDoc = XDocument.Load(packagesConfig);
            if (packagesDoc.Descendants("package")
                .Any(p => p.Attribute("id")?.Value?.StartsWith("Microsoft.AspNet") == true))
                return true;
        }

        return false;
    }

    private bool HasMvcIndicators(string projectDir, XDocument doc)
    {
        // Check for MVC packages
        var packagesConfig = Path.Combine(projectDir, "packages.config");
        if (File.Exists(packagesConfig))
        {
            try
            {
                var packagesDoc = XDocument.Load(packagesConfig);
                if (packagesDoc.Descendants("package")
                    .Any(p => p.Attribute("id")?.Value?.Contains("Microsoft.AspNet.Mvc") == true))
                    return true;
            }
            catch (IOException)
            {
                // Log or handle IO errors
            }
            catch (Exception)
            {
                // Log or handle other exceptions
            }
        }

        // Check for Controllers folder
        var controllersDir = Path.Combine(projectDir, "Controllers");
        if (Directory.Exists(controllersDir) && Directory.GetFiles(controllersDir, "*.cs").Any())
            return true;

        // Check for Views folder
        var viewsDir = Path.Combine(projectDir, "Views");
        if (Directory.Exists(viewsDir) && Directory.GetFiles(viewsDir, "*.cshtml", SearchOption.AllDirectories).Any())
            return true;

        return false;
    }

    private bool HasWebFormsIndicators(string projectDir)
    {
        // Check for .aspx files
        if (Directory.GetFiles(projectDir, "*.aspx", SearchOption.AllDirectories).Any())
            return true;

        // Check for .ascx files (user controls)
        if (Directory.GetFiles(projectDir, "*.ascx", SearchOption.AllDirectories).Any())
            return true;

        return false;
    }

    private bool HasReference(XDocument doc, string referenceName)
    {
        return doc.Descendants("Reference")
            .Any(r => r.Attribute("Include")?.Value?.StartsWith(referenceName) == true);
    }

    private string? FindWcfServiceFile(string projectDir)
    {
        var svcFiles = Directory.GetFiles(projectDir, "*.svc", SearchOption.AllDirectories);
        return svcFiles.FirstOrDefault();
    }

    private string? FindGlobalAsax(string projectDir)
    {
        var globalAsax = Path.Combine(projectDir, "Global.asax.cs");
        if (File.Exists(globalAsax))
            return globalAsax;

        globalAsax = Path.Combine(projectDir, "Global.asax");
        if (File.Exists(globalAsax))
            return globalAsax;

        return null;
    }
}
