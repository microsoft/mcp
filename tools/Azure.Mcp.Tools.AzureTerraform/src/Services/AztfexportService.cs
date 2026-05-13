// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class AztfexportService : IAztfexportService
{
    public async Task<bool> IsAztfexportAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var checkCommand = OperatingSystem.IsWindows() ? "where" : "which";

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = checkCommand,
                Arguments = "aztfexport",
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

    public AztfexportCommandResult GenerateResourceCommand(
        string resourceId,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? resourceName = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true)
    {
        parallelism = Math.Clamp(parallelism, 1, 50);
        var args = new List<string> { "resource", "--non-interactive", "--plain-ui" };

        if (string.Equals(provider, "azapi", StringComparison.OrdinalIgnoreCase))
        {
            args.AddRange(["--provider-name", "azapi"]);
        }

        if (!string.IsNullOrEmpty(resourceName))
        {
            args.AddRange(["--name", resourceName]);
        }

        if (includeRoleAssignment)
        {
            args.Add("--include-role-assignment");
        }

        args.AddRange(["--parallelism", parallelism.ToString(System.Globalization.CultureInfo.InvariantCulture)]);

        if (continueOnError)
        {
            args.Add("--continue");
        }

        args.Add(resourceId);

        return new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = args,
            Description = $"Export Azure resource: {resourceId}",
            OutputFolderName = outputFolderName,
            WorkingDirectory = outputFolderName,
            Notes =
            [
                "This command exports a single Azure resource to Terraform configuration.",
                outputFolderName is not null
                    ? $"Output will be saved to: {outputFolderName}"
                    : "Output will be saved to the current working directory",
                $"Using provider: {provider}"
            ]
        };
    }

    public AztfexportCommandResult GenerateResourceGroupCommand(
        string resourceGroupName,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? namePattern = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true)
    {
        parallelism = Math.Clamp(parallelism, 1, 50);
        var args = new List<string> { "resource-group", "--non-interactive", "--plain-ui" };

        if (string.Equals(provider, "azapi", StringComparison.OrdinalIgnoreCase))
        {
            args.AddRange(["--provider-name", "azapi"]);
        }

        if (!string.IsNullOrEmpty(namePattern))
        {
            args.AddRange(["--name-pattern", namePattern]);
        }

        if (includeRoleAssignment)
        {
            args.Add("--include-role-assignment");
        }

        args.AddRange(["--parallelism", parallelism.ToString(System.Globalization.CultureInfo.InvariantCulture)]);

        if (continueOnError)
        {
            args.Add("--continue");
        }

        args.Add(resourceGroupName);

        return new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = args,
            Description = $"Export Azure resource group: {resourceGroupName}",
            OutputFolderName = outputFolderName,
            WorkingDirectory = outputFolderName,
            Notes =
            [
                "This command exports an entire Azure resource group and all its resources to Terraform configuration.",
                outputFolderName is not null
                    ? $"Output will be saved to: {outputFolderName}"
                    : "Output will be saved to the current working directory",
                $"Using provider: {provider}"
            ]
        };
    }

    public AztfexportCommandResult GenerateQueryCommand(
        string query,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? namePattern = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true)
    {
        parallelism = Math.Clamp(parallelism, 1, 50);
        var args = new List<string> { "query", "--non-interactive", "--plain-ui" };

        if (string.Equals(provider, "azapi", StringComparison.OrdinalIgnoreCase))
        {
            args.AddRange(["--provider-name", "azapi"]);
        }

        if (!string.IsNullOrEmpty(namePattern))
        {
            args.AddRange(["--name-pattern", namePattern]);
        }

        if (includeRoleAssignment)
        {
            args.Add("--include-role-assignment");
        }

        args.AddRange(["--parallelism", parallelism.ToString(System.Globalization.CultureInfo.InvariantCulture)]);

        if (continueOnError)
        {
            args.Add("--continue");
        }

        args.Add(query);

        var queryPreview = query.Length > 50 ? string.Concat(query.AsSpan(0, 50), "...") : query;

        return new AztfexportCommandResult
        {
            AztfexportFound = true,
            Command = "aztfexport",
            Args = args,
            Description = $"Export Azure resources by query: {queryPreview}",
            OutputFolderName = outputFolderName,
            WorkingDirectory = outputFolderName,
            Notes =
            [
                "This command exports Azure resources matching the given Azure Resource Graph query to Terraform configuration.",
                outputFolderName is not null
                    ? $"Output will be saved to: {outputFolderName}"
                    : "Output will be saved to the current working directory",
                $"Query: {query}",
                $"Using provider: {provider}"
            ]
        };
    }

    internal static InstallationHelp GetAztfexportInstallationHelp() => new()
    {
        ToolName = "aztfexport",
        DocumentationUrl = "https://github.com/Azure/aztfexport#install",
        VerifyCommand = "aztfexport --version",
        InstallationMethods =
        [
            new() { Platform = "windows", Method = "winget", Command = "winget install aztfexport", ManagesPath = true },
            new() { Platform = "macos", Method = "brew", Command = "brew install aztfexport", ManagesPath = true },
            new() { Platform = "linux", Method = "brew", Command = "brew install aztfexport", ManagesPath = true },
            new() { Platform = "linux", Method = "apt", Command = "curl -sSL https://packages.microsoft.com/keys/microsoft.asc > /etc/apt/trusted.gpg.d/microsoft.asc && apt-add-repository https://packages.microsoft.com/ubuntu/22.04/prod && apt-get update && apt-get install -y aztfexport", ManagesPath = true },
            new() { Platform = "linux", Method = "dnf", Command = "rpm --import https://packages.microsoft.com/keys/microsoft.asc && dnf install -y https://packages.microsoft.com/config/rhel/9/packages-microsoft-prod.rpm && dnf install -y aztfexport", ManagesPath = true },
            new() { Platform = "windows", Method = "manual", Command = "Download the zip for your architecture from https://github.com/Azure/aztfexport/releases, extract it, and add the folder containing aztfexport.exe to your system PATH.", ManagesPath = false },
            new() { Platform = "linux", Method = "manual", Command = "Download the zip for your architecture from https://github.com/Azure/aztfexport/releases, extract it, and move the aztfexport binary to a directory in your PATH (e.g., /usr/local/bin).", ManagesPath = false },
            new() { Platform = "macos", Method = "manual", Command = "Download the zip for your architecture from https://github.com/Azure/aztfexport/releases, extract it, and move the aztfexport binary to /usr/local/bin/.", ManagesPath = false }
        ],
        AdditionalNotes =
        [
            "aztfexport requires Terraform (>= v0.12) to be installed and available in PATH.",
            "Ensure you are authenticated to Azure before running export commands (e.g., run: az login).",
            "On Windows you may need to restart your terminal/shell after installation for PATH changes to take effect."
        ]
    };

    internal static AztfexportCommandResult NotFoundResult(string description) => new()
    {
        AztfexportFound = false,
        Command = string.Empty,
        Description = description,
        Notes = ["aztfexport is not installed or not available in PATH. Install it using the instructions below, then retry."],
        InstallationHelp = GetAztfexportInstallationHelp()
    };
}
