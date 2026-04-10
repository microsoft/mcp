// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class ConftestService : IConftestService
{
    private const string PolicyRepoUrl = "https://github.com/Azure/policy-library-avm.git";
    private const string PolicyDirectory = "policy";

    public async Task<bool> IsConftestAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var checkCommand = OperatingSystem.IsWindows() ? "where" : "which";

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = checkCommand,
                Arguments = "conftest",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public ConftestCommandResult GenerateWorkspaceValidationCommand(
        string workspaceFolder,
        string policySet = "all",
        string? severityFilter = null,
        string? customPolicies = null)
    {
        var args = new List<string> { "test", "--all-namespaces", "--output", "json" };

        AddPolicyArgs(args, policySet, severityFilter, customPolicies);

        // Target is the current directory (workspace folder contains .tf files)
        args.Add(".");

        return new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = args,
            Description = $"Validate Terraform workspace: {workspaceFolder}",
            WorkspaceFolder = workspaceFolder,
            WorkingDirectory = workspaceFolder,
            PolicySet = policySet,
            Notes = BuildNotes("workspace", workspaceFolder, policySet, severityFilter)
        };
    }

    public ConftestCommandResult GeneratePlanValidationCommand(
        string planFolder,
        string policySet = "all",
        string? severityFilter = null,
        string? customPolicies = null)
    {
        var args = new List<string> { "test", "--all-namespaces", "--output", "json" };

        AddPolicyArgs(args, policySet, severityFilter, customPolicies);

        // Target is the plan JSON file
        args.Add("tfplan.json");

        return new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = args,
            Description = $"Validate Terraform plan in: {planFolder}",
            WorkspaceFolder = planFolder,
            WorkingDirectory = planFolder,
            PolicySet = policySet,
            Notes = BuildNotes("plan", planFolder, policySet, severityFilter)
        };
    }

    internal static void AddPolicyArgs(List<string> args, string policySet, string? severityFilter, string? customPolicies)
    {
        var policyPath = policySet switch
        {
            "Azure-Proactive-Resiliency-Library-v2" => $"./{PolicyDirectory}/Azure-Proactive-Resiliency-Library-v2",
            "avmsec" => $"./{PolicyDirectory}/avmsec",
            _ => $"./{PolicyDirectory}"
        };

        args.AddRange(["-p", policyPath]);

        if (string.Equals(policySet, "avmsec", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(severityFilter))
        {
            args.AddRange(["-p", $".conftest_severity_{severityFilter}.rego"]);
        }

        if (!string.IsNullOrEmpty(customPolicies))
        {
            foreach (var policy in customPolicies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                args.AddRange(["-p", policy]);
            }
        }
    }

    internal static List<string> BuildNotes(string validationType, string folder, string policySet, string? severityFilter)
    {
        var notes = new List<string>
        {
            validationType == "workspace"
                ? "This command validates Terraform .tf files in the workspace against Azure policies using conftest."
                : "This command validates a Terraform plan JSON file (tfplan.json) against Azure policies using conftest.",
            $"Working directory: {folder}",
            $"Policy set: {policySet}",
            $"Before running this command, ensure the Azure policy library is cloned into the workspace:",
            $"  git clone {PolicyRepoUrl} {PolicyDirectory}"
        };

        if (string.Equals(policySet, "avmsec", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(severityFilter))
        {
            notes.Add($"Severity filter: {severityFilter}");
        }

        if (validationType == "plan")
        {
            notes.Add("Ensure you have generated a plan JSON file first: terraform plan -out=tfplan && terraform show -json tfplan > tfplan.json");
        }

        return notes;
    }

    internal static InstallationHelp GetConftestInstallationHelp() => new()
    {
        ToolName = "conftest",
        DocumentationUrl = "https://www.conftest.dev/install/",
        VerifyCommand = "conftest --version",
        InstallationMethods =
        [
            new() { Platform = "windows", Method = "scoop", Command = "scoop install conftest", ManagesPath = true },
            new() { Platform = "windows", Method = "choco", Command = "choco install conftest", ManagesPath = true },
            new() { Platform = "macos", Method = "brew", Command = "brew install conftest", ManagesPath = true },
            new() { Platform = "linux", Method = "brew", Command = "brew install conftest", ManagesPath = true },
            new() { Platform = "linux", Method = "apt", Command = "wget https://github.com/open-policy-agent/conftest/releases/latest/download/conftest_Linux_x86_64.deb && sudo dpkg -i conftest_Linux_x86_64.deb", ManagesPath = true },
            new() { Platform = "linux", Method = "dnf", Command = "wget https://github.com/open-policy-agent/conftest/releases/latest/download/conftest_Linux_x86_64.rpm && sudo rpm -ivh conftest_Linux_x86_64.rpm", ManagesPath = true },
            new() { Platform = "windows", Method = "manual", Command = "Download the zip for your architecture from https://github.com/open-policy-agent/conftest/releases, extract it, and add the folder containing conftest.exe to your system PATH.", ManagesPath = false },
            new() { Platform = "linux", Method = "manual", Command = "Download the tar.gz for your architecture from https://github.com/open-policy-agent/conftest/releases, extract it, and move the conftest binary to a directory in your PATH (e.g., /usr/local/bin).", ManagesPath = false },
            new() { Platform = "macos", Method = "manual", Command = "Download the tar.gz for your architecture from https://github.com/open-policy-agent/conftest/releases, extract it, and move the conftest binary to /usr/local/bin/.", ManagesPath = false }
        ],
        AdditionalNotes =
        [
            "conftest is used to validate Terraform configurations against Azure policies using the Open Policy Agent (OPA) framework.",
            $"After installing conftest, clone the Azure policy library into your workspace: git clone {PolicyRepoUrl} {PolicyDirectory}",
            "On Windows you may need to restart your terminal/shell after installation for PATH changes to take effect."
        ]
    };

    internal static ConftestCommandResult NotFoundResult(string description) => new()
    {
        ConftestFound = false,
        Command = string.Empty,
        Description = description,
        Notes = ["conftest is not installed or not available in PATH. Install it using the instructions below, then retry."],
        InstallationHelp = GetConftestInstallationHelp()
    };
}
