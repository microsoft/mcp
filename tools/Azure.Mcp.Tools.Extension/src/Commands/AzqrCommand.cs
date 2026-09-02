// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Time;

namespace Azure.Mcp.Tools.Extension.Commands;

[CommandMetadata(
    Id = "e7ef18a3-2730-4300-bad3-dc766f47dd2a",
    Name = "azqr",
    Title = "Azure Quick Review CLI Command",
    Description = "Runs Azure Quick Review CLI (azqr) to scan an Azure subscription (or resource group) for compliance issues and provide compliance recommendations. Generates a compliance and security assessment report that identifies non-compliant configurations and recommends improvements for your Azure resources. Use this whenever a user wants to scan, check, review, or assess a subscription for compliance issues or compliance recommendations, or wants recommendations to fix compliance and security problems. Requires a subscription (ID or name) and optionally a resource group. Returns the generated report file paths (XLSX and JSON). Note: azqr performs compliance and security scans and is different from Azure CLI (az), from Azure Policy assignments, and from Azure Advisor recommendations for cost, security, reliability, operational excellence, and performance improvements.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class AzqrCommand(
    ILogger<AzqrCommand> logger,
    IAzureService azureService,
    IAzqrCliService azqrCliService,
    IDateTimeProvider dateTimeProvider,
    IExternalProcessService processService,
    ISubscriptionResolver subscriptionResolver,
    int processTimeoutSeconds = 300)
    : SubscriptionCommand<AzqrOptions, AzqrReportResult>(subscriptionResolver)
{
    private readonly ILogger<AzqrCommand> _logger = logger;
    private readonly IAzureService _azureService = azureService;
    private readonly IAzqrCliService _azqrCliService = azqrCliService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IExternalProcessService _processService = processService;
    private readonly int _processTimeoutSeconds = processTimeoutSeconds;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AzqrOptions options, CancellationToken cancellationToken)
    {
        var response = context.Response;

        try
        {
            var azqrPath = _azqrCliService.GetSupportedExecutablePath() ?? throw new FileNotFoundException("Azure Quick Review CLI (azqr) version 3.0.0 or later was not found in PATH. Please ensure a supported version of azqr is installed. Go to https://aka.ms/azqr to learn more about how to install Azure Quick Review CLI.");

            var subscription = await _azureService.GetSubscription(options.Subscription!, options.Tenant, cancellationToken: cancellationToken);

            // Compose azqr command
            var command = $"scan --subscription-id {subscription.Id}";
            if (!string.IsNullOrWhiteSpace(options.ResourceGroup))
            {
                command += $" --resource-group {options.ResourceGroup}";
            }

            var tempDir = Path.GetTempPath();
            var dateString = _dateTimeProvider.UtcNow.ToString("yyyyMMdd-HHmmss");
            var reportFileName = Path.Combine(tempDir, $"azqr-report-{options.Subscription}-{dateString}");

            // Azure Quick Review always appends the file extension to the report file's name, we need to create a new path with the file extension to check for the existence of the report file.
            var xlsxReportFilePath = $"{reportFileName}.xlsx";
            var jsonReportFilePath = $"{reportFileName}.json";
            command += $" --output-name \"{reportFileName}\"";

            // Also generate a JSON report for users who don't have access to Excel.
            command += " --json";

            var result = await _processService.ExecuteAsync(azqrPath, command,
                operationTimeoutSeconds: _processTimeoutSeconds,
                cancellationToken: cancellationToken);

            if (result.ExitCode != 0)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Message = result.Error;
                return response;
            }

            if (!File.Exists(xlsxReportFilePath) && !File.Exists(jsonReportFilePath))
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.Message = $"Report file '{xlsxReportFilePath}' and '{jsonReportFilePath}' were not found after azqr execution.";
                return response;
            }
            var resultObj = new AzqrReportResult(xlsxReportFilePath, jsonReportFilePath, result.Output);
            response.Results = ResponseResult.Create(resultObj, ExtensionJsonContext.Default.AzqrReportResult);
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

}
