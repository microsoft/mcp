// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;
using Azure.Mcp.Tools.Monitor.Options.Ingestion;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Ingestion;

[CommandMetadata(
    Id = "b3a6f0d2-7c41-4e8a-9d15-2f6e8c0a41b7",
    Name = "upload",
    Title = "Upload Custom Logs to Azure Monitor",
    Description = """
        Uploads custom log data to Azure Monitor (Log Analytics) through the Logs Ingestion API using a data collection rule.
        Requires ingestionEndpoint (data collection endpoint URL), dataCollectionRule (immutable ID, 'dcr-...'), stream
        (for example 'Custom-MyTable_CL'), and logData (JSON array of objects, up to 1 MB). The data is checked for valid
        JSON array shape before upload. Returns the ingestion status and the number of records uploaded.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class IngestionUploadCommand(
    ILogger<IngestionUploadCommand> logger,
    IMonitorIngestionService ingestionService)
    : AuthenticatedCommand<IngestionUploadOptions, IngestionUploadResult>
{
    private readonly ILogger<IngestionUploadCommand> _logger = logger;
    private readonly IMonitorIngestionService _ingestionService = ingestionService;

    public override JsonTypeInfo<IngestionUploadResult>? ResultTypeInfo =>
        MonitorJsonContext.Default.IngestionUploadResult;

    public override void ValidateOptions(IngestionUploadOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var shape = IngestionLogDataValidator.Validate(options.LogData, schema: null);
        if (shape.Errors.Count > 0)
        {
            validationResult.AddError(
                string.Join(" ", shape.Errors.Select(e => e.Message)),
                "Invalid ingestion log data.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IngestionUploadOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _ingestionService.UploadLogsAsync(
                options.IngestionEndpoint,
                options.DataCollectionRule,
                options.Stream,
                options.LogData,
                options.Tenant,
                cancellationToken);

            SetResult(context, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Log ingestion upload failed. DataCollectionRule: {DataCollectionRule}, Stream: {Stream}",
                options.DataCollectionRule, options.Stream);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
