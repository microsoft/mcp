// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;
using Azure.Mcp.Tools.Monitor.Options.Ingestion;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Ingestion;

[CommandMetadata(
    Id = "e7d19c35-5a82-4b0f-8c63-91f4a2d7b6e0",
    Name = "validate",
    Title = "Validate Log Data for Azure Monitor Ingestion",
    Description = """
        Validates log data before ingestion into Azure Monitor to prevent upload errors. Checks that the data is a JSON array
        of objects within the 1 MB request limit and compares each record against the stream schema declared in a data
        collection rule (column types, undeclared columns, and missing columns). Requires resourceGroup, dataCollectionRule
        (rule name), and logData; stream is optional when the rule declares exactly one stream. Returns validation results
        with error and warning details as JSON. Does not upload data.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IngestionValidateCommand(
    ILogger<IngestionValidateCommand> logger,
    IMonitorIngestionService ingestionService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IngestionValidateOptions, IngestionValidationResult>(subscriptionResolver)
{
    private readonly ILogger<IngestionValidateCommand> _logger = logger;
    private readonly IMonitorIngestionService _ingestionService = ingestionService;

    public override JsonTypeInfo<IngestionValidationResult>? ResultTypeInfo =>
        MonitorJsonContext.Default.IngestionValidationResult;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IngestionValidateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _ingestionService.ValidateLogDataAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.DataCollectionRule,
                options.Stream,
                options.LogData,
                options.Tenant,
                cancellationToken);

            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Log data validation failed. ResourceGroup: {ResourceGroup}, DataCollectionRule: {DataCollectionRule}",
                options.ResourceGroup, options.DataCollectionRule);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
