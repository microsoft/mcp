// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Monitor.Ingestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Azure.Mcp.Tools.Monitor.LiveTests;

/// <summary>
/// Helper class for sending logs to Azure Log Analytics using the Azure Monitor Ingestion SDK.
/// </summary>
/// <remarks>
/// Initializes a new instance of the LogAnalyticsHelper class.
/// </remarks>
public class LogAnalyticsHelper(
    string workspaceName,
    string subscription,
    IMonitorService monitorService,
    string? tenantId = null,
    string logType = "TestLogs_CL",
    ILogger? logger = null)
{
    private readonly string _workspaceName = workspaceName;
    private readonly string _subscription = subscription;
    private readonly string _logType = logType;
    private readonly string? _tenantId = tenantId;
    private readonly TokenCredential _credential = new CustomChainedCredential(tenantId);
    private readonly IMonitorService _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
    private readonly ILogger _logger = logger ?? NullLogger.Instance;
    private readonly SemaphoreSlim _clientInitLock = new(1, 1);
    private string? _workspaceId;
    private LogsIngestionClient? _logsIngestionClient;

    private async Task<string> GetWorkspaceIdAsync()
    {
        if (!string.IsNullOrEmpty(_workspaceId))
        {
            return _workspaceId;
        }

        // Get workspace info using the monitor service
        var workspaces = await _monitorService.ListWorkspaces(_subscription, _tenantId);
        var workspace = workspaces.FirstOrDefault(w => w.Name.Equals(_workspaceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Could not find workspace {_workspaceName}");

        _workspaceId = workspace.CustomerId;
        return _workspaceId;
    }

    private async Task<LogsIngestionClient> GetLogsIngestionClientAsync(string customerId)
    {
        if (_logsIngestionClient != null)
        {
            return _logsIngestionClient;
        }

        if (string.IsNullOrEmpty(customerId))
        {
            throw new ArgumentNullException(nameof(customerId), "Customer ID cannot be null or empty");
        }

        await _clientInitLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_logsIngestionClient != null)
            {
                return _logsIngestionClient;
            }

            var endpoint = new Uri($"https://{customerId}.ods.opinsights.azure.com");
            var options = new LogsIngestionClientOptions
            {
                Retry =
                {
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(10)
                }
            };

            _logsIngestionClient = new LogsIngestionClient(endpoint, _credential, options);
            return _logsIngestionClient;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create LogsIngestionClient: {ex.Message}", ex);
        }
        finally
        {
            _clientInitLock.Release();
        }
    }

    /// <summary>
    /// Creates and sends a log with the specified level and message.
    /// </summary>
    private async Task<HttpStatusCode> CreateAndSendLogAsync(
        string level,
        string message,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = await GetWorkspaceIdAsync().ConfigureAwait(false);

        var log = new LogRecord
        {
            TimeGenerated = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            Application = "MonitorCommandTests"
        };

        return await SendLogAsync(workspaceId, [log], cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an information level log message.
    /// </summary>
    public Task<HttpStatusCode> SendInfoLogAsync(CancellationToken cancellationToken = default)
    {
        return CreateAndSendLogAsync(
            "Information",
            $"Test info message: {DateTimeOffset.UtcNow:O}",
            cancellationToken);
    }

    /// <summary>
    /// Sends both information and error test logs.
    /// </summary>
    public async Task<(HttpStatusCode infoStatus, HttpStatusCode errorStatus)> SendTestLogsAsync(
        string testId,
        CancellationToken cancellationToken = default)
    {
        var infoStatus = await CreateAndSendLogAsync(
            "Information",
            $"Test info message: {testId}",
            cancellationToken).ConfigureAwait(false);

        var errorStatus = await CreateAndSendLogAsync(
            "Error",
            $"Test error message {Guid.NewGuid()}",
            cancellationToken).ConfigureAwait(false);

        return (infoStatus, errorStatus);
    }

    /// <summary>
    /// Sends log records to Azure Monitor using the Logs Ingestion API.
    /// </summary>
    private async Task<HttpStatusCode> SendLogAsync(
        string customerId,
        LogRecord[] logs,
        CancellationToken cancellationToken = default)
    {
        var client = await GetLogsIngestionClientAsync(customerId).ConfigureAwait(false);

        try
        {
            using var content = RequestContent.Create(logs);
            _logger.LogInformation("Sending {Count} logs to workspace {WorkspaceId}", logs.Length, customerId);

            cancellationToken.ThrowIfCancellationRequested();
            var response = await client.UploadAsync(
                customerId,  // DCR rule ID
                _logType,   // Stream name (table name)
                content,    // Log data
                null,       // No content type (defaults to application/json)
                default    // No request context
            ).ConfigureAwait(false);

            var status = (HttpStatusCode)response.Status;
            _logger.LogInformation("Log upload completed with status {Status}", status);
            return status;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error sending logs to workspace {WorkspaceId}", customerId);
            return HttpStatusCode.InternalServerError;
        }
    }
}

public class LogRecord
{
    [JsonPropertyName("TimeGenerated")]
    public DateTimeOffset TimeGenerated { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("Level")]
    public string Level { get; set; } = "";

    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("Application")]
    public string Application { get; set; } = "";

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
}
