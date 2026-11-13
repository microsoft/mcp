// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed partial class ResilienceManagementService
{
    private static readonly TimeSpan DrillReportOperationTimeout = TimeSpan.FromMinutes(10);

    public async Task<DrillRunGenerateReportResult> GenerateDrillRunReportAsync(
        string serviceGroup,
        string drill,
        string drillRun,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(
            tenantIdOrName: tenant,
            armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(),
            cancellationToken: cancellationToken);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(
            DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun));
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<DrillReportSummary> operation =
            await drillRunResource.GenerateReportAsync(WaitUntil.Started, operationId, cancellationToken);
        DrillReportSummary? report = operation.HasValue
            ? operation.Value
            : ReadDrillRunReport(operation.GetRawResponse()?.Content);

        return new DrillRunGenerateReportResult(
            operationId,
            operation.HasCompleted,
            report is null ? null : MapDrillRunReport(report));
    }

    internal static DrillReportSummary? ReadDrillRunReport(BinaryData? rawOperationResponse)
    {
        if (rawOperationResponse is null)
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawOperationResponse);
            if (!document.RootElement.TryGetProperty("properties", out JsonElement properties) ||
                !properties.TryGetProperty("report", out JsonElement report) ||
                report.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ModelReaderWriter.Read<DrillReportSummary>(
                BinaryData.FromString(report.GetRawText()),
                ModelReaderWriterOptions.Json,
                AzureResourceManagerResilienceManagementContext.Default);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<DrillRunReportDownloadInfo> GetDrillRunReportDownloadUrlAsync(
        string serviceGroup,
        string drill,
        string drillRun,
        string? format = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(
            tenantIdOrName: tenant,
            armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(),
            cancellationToken: cancellationToken);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(
            DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun));
        var content = new ListReportDownloadUrlContent();
        if (format is not null)
        {
            content.Format = new DrillReportFormat(format);
        }

        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ListReportDownloadUrlResult> operation =
            await drillRunResource.GetReportDownloadUriAsync(WaitUntil.Started, operationId, content, cancellationToken);
        await ExecuteWithTimeoutAsync(
            async token =>
            {
                await WaitForLroCompletionAsync(operation, token);
                return true;
            },
            "drill run report download URL request",
            DrillReportOperationTimeout,
            cancellationToken);

        return MapDrillRunReportDownload(operation.Value, format, operation.GetRawResponse()?.Content);
    }

    internal static DrillRunReportInfo MapDrillRunReport(DrillReportSummary summary) => new(
        summary.GenerationStatus?.ToString(),
        summary.FinalizationState?.ToString(),
        summary.StageStatuses
            .Select(stage => new DrillRunReportStageInfo(
                stage.DrillRunStage.ToString(),
                stage.GenerationStatus?.ToString(),
                stage.LastAttemptOn,
                MapDrillRunReportError(stage.LastError)))
            .ToList(),
        summary.AvailableFormats.Select(format => format.ToString()).ToList(),
        summary.LastGeneratedOn,
        summary.SchemaVersion,
        MapDrillRunReportError(summary.LastError));

    internal static DrillRunReportDownloadInfo MapDrillRunReportDownload(
        ListReportDownloadUrlResult result,
        string? requestedFormat,
        BinaryData? rawOperationResponse = null)
    {
        JsonElement? properties = ReadReportDownloadProperties(rawOperationResponse);
        string? downloadUrl = result.DownloadUri?.ToString() ?? ReadString(properties, "downloadUrl");
        if (string.IsNullOrEmpty(downloadUrl))
        {
            throw new InvalidOperationException("The service returned no report download URL.");
        }

        DateTimeOffset? expiresOn = result.ExpiryOn;
        if (expiresOn is null &&
            DateTimeOffset.TryParse(
                ReadString(properties, "expiryTimestamp"),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out DateTimeOffset parsedExpiry))
        {
            expiresOn = parsedExpiry;
        }

        return new(
            result.Format?.ToString() ?? ReadString(properties, "format") ?? requestedFormat,
            downloadUrl,
            expiresOn);
    }

    private static JsonElement? ReadReportDownloadProperties(BinaryData? rawOperationResponse)
    {
        if (rawOperationResponse is null)
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawOperationResponse);
            return document.RootElement.TryGetProperty("properties", out JsonElement properties) &&
                properties.ValueKind == JsonValueKind.Object
                    ? properties.Clone()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement? element, string propertyName) =>
        element is { } value &&
        value.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static DrillRunReportErrorInfo? MapDrillRunReportError(ResilienceManagementErrorDetail? error) =>
        error is null
            ? null
            : new DrillRunReportErrorInfo(error.Code, error.Message, error.Recommendations.ToList());
}
