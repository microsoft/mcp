// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Time;
using Azure.Mcp.Tools.Extension.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Extension.Commands;

public sealed class AzqrCommand(ILogger<AzqrCommand> logger, int processTimeoutSeconds = 300) : SubscriptionCommand<AzqrOptions>()
{
    private const string CommandTitle = "Azure Quick Review CLI Command";
    private readonly ILogger<AzqrCommand> _logger = logger;
    private readonly int _processTimeoutSeconds = processTimeoutSeconds;
    private static string? _cachedAzqrPath;

    public override string Name => "azqr";

    public override string Description =>
        """
        Runs Azure Quick Review CLI (azqr) commands to generate compliance/security reports for Azure resources.
        This tool should be used when the user wants to identify any non-compliant configurations or areas for improvement in their Azure resources.
        Requires a subscription id and optionally a resource group name. Returns the generated report file's path.
        Note that Azure Quick Review CLI (azqr) is different from Azure CLI (az).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup();
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        var response = context.Response;
        try
        {
            if (!Validate(parseResult.CommandResult, response).IsValid)
            {
                return response;
            }

            ArgumentNullException.ThrowIfNull(options.Subscription);

            var azqrPath = FindAzqrCliPath() ?? throw new FileNotFoundException("Azure Quick Review CLI (azqr) executable not found in PATH. Please ensure azqr is installed. Go to https://aka.ms/azqr to learn more about how to install Azure Quick Review CLI.");

            var subscriptionService = context.GetService<ISubscriptionService>();
            var dateTimeProvider = context.GetService<IDateTimeProvider>();
            var subscription = await subscriptionService.GetSubscription(options.Subscription, options.Tenant);

            // Compose azqr command
            var command = $"scan --subscription-id {subscription.Id}";
            if (!string.IsNullOrWhiteSpace(options.ResourceGroup))
            {
                command += $" --resource-group {options.ResourceGroup}";
            }

            var tempDir = Path.GetTempPath();
            var dateString = dateTimeProvider.UtcNow.ToString("yyyyMMdd-HHmmss");
            var reportFileName = Path.Combine(tempDir, $"azqr-report-{options.Subscription}-{dateString}");

            // Azure Quick Review always appends the file extension to the report file's name, we need to create a new path with the file extension to check for the existence of the report file.
            var xlsxReportFilePath = $"{reportFileName}.xlsx";
            var jsonReportFilePath = $"{reportFileName}.json";
            command += $" --output-name \"{reportFileName}\"";

            // Azure Quick Review CLI can easily get throttle errors when scanning subscriptions with many resources with costs enabled.
            // Unfortunately, getting such an error will abort the entire job and waste all the partial results.
            // To reduce the chance of throttling, we disable costs reporting by default.
            command += " --costs=false";

            // Also generate a JSON report for users who don't have access to Excel.
            command += " --json";

            var processService = context.GetService<IExternalProcessService>();
            var result = await processService.ExecuteAsync(azqrPath, command, _processTimeoutSeconds);

            if (result.ExitCode != 0)
            {
                response.Status = 500;
                response.Message = result.Error;
                return response;
            }

            if (!File.Exists(xlsxReportFilePath) && !File.Exists(jsonReportFilePath))
            {
                response.Status = 500;
                response.Message = $"Report file '{xlsxReportFilePath}' and '{jsonReportFilePath}' were not found after azqr execution.";
                return response;
            }
            var resultObj = new AzqrReportResult(xlsxReportFilePath, jsonReportFilePath, result.Output);
            response.Results = ResponseResult.Create(resultObj, ExtensionJsonContext.Default.AzqrReportResult);
            response.Status = 200;
            response.Message = "azqr report generated successfully.";
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred executing azqr command.");
            HandleException(context, ex);
            return response;
        }
    }

    private static string? FindAzqrCliPath()
    {
        // Return cached path if available and still exists
        if (!string.IsNullOrEmpty(_cachedAzqrPath) && File.Exists(_cachedAzqrPath))
        {
            return _cachedAzqrPath;
        }
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "azqr.exe" : "azqr";
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(dir.Trim(), exeName);
            if (File.Exists(fullPath))
            {
                _cachedAzqrPath = fullPath;
                return fullPath;
            }
        }
        return null;
    }
}
