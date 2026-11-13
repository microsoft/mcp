// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class DrillRunReportMappingTests
{
    [Fact]
    public void MapDrillRunReport_MapsSummaryStagesFormatsAndErrors()
    {
        var generatedOn = DateTimeOffset.Parse("2026-10-22T01:00:00Z");
        var attemptedOn = DateTimeOffset.Parse("2026-10-22T00:55:00Z");
        DrillReportSummary summary = ArmResilienceManagementModelFactory.DrillReportSummary(
            generationStatus: DrillReportGenerationStatus.Failed,
            stageStatuses:
            [
                ArmResilienceManagementModelFactory.DrillReportStageStatus(
                    DrillRunStage.FaultInjection,
                    DrillReportGenerationStatus.Succeeded,
                    attemptedOn),
                ArmResilienceManagementModelFactory.DrillReportStageStatus(
                    DrillRunStage.Failover,
                    DrillReportGenerationStatus.Failed,
                    attemptedOn,
                    ArmResilienceManagementModelFactory.ResilienceManagementErrorDetail("StageFailed", "Stage failed", ["Retry"]))
            ],
            availableFormats: [DrillReportFormat.Html],
            lastGeneratedOn: generatedOn,
            schemaVersion: "1.0",
            finalizationState: DrillReportFinalizationState.NotFinalized,
            lastError: ArmResilienceManagementModelFactory.ResilienceManagementErrorDetail("ReportFailed", "Report failed"));

        var report = ResilienceManagementService.MapDrillRunReport(summary);

        Assert.Equal("Failed", report.GenerationStatus);
        Assert.Equal("NotFinalized", report.FinalizationState);
        Assert.Equal("Html", Assert.Single(report.AvailableFormats));
        Assert.Equal(generatedOn, report.LastGeneratedOn);
        Assert.Equal("1.0", report.SchemaVersion);
        Assert.Equal("ReportFailed", report.LastError?.Code);
        Assert.Equal(2, report.StageStatuses.Count);
        Assert.Equal("FaultInjection", report.StageStatuses[0].DrillRunStage);
        Assert.Null(report.StageStatuses[0].LastError);
        Assert.Equal("Failover", report.StageStatuses[1].DrillRunStage);
        Assert.Equal("StageFailed", report.StageStatuses[1].LastError?.Code);
        Assert.Equal("Retry", Assert.Single(report.StageStatuses[1].LastError!.Recommendations));
    }

    [Fact]
    public void MapDrillRunReport_HandlesEmptySummary()
    {
        var report = ResilienceManagementService.MapDrillRunReport(ArmResilienceManagementModelFactory.DrillReportSummary());

        Assert.Null(report.GenerationStatus);
        Assert.Empty(report.StageStatuses);
        Assert.Empty(report.AvailableFormats);
        Assert.Null(report.LastError);
    }

    [Fact]
    public void ReadDrillRunReport_ReadsSummaryNestedInDrillRunEnvelope()
    {
        // Shape of the 202 response from generateReport: the DrillRun resource with the summary at properties.report.
        var raw = BinaryData.FromString("""
            {
              "id": "/providers/Microsoft.Management/serviceGroups/sg/providers/Microsoft.AzureResilienceManagement/drills/d/drillRuns/r",
              "properties": {
                "jobType": "DrillRun",
                "report": {
                  "generationStatus": "Succeeded",
                  "stageStatuses": [
                    { "drillRunStage": "Failover", "generationStatus": "Succeeded", "lastAttemptTimestamp": "2026-10-20T11:29:47Z", "lastError": null }
                  ],
                  "availableFormats": ["Html"],
                  "lastGeneratedTimestamp": "2026-10-20T11:32:33Z",
                  "schemaVersion": "1",
                  "finalizationState": "Finalized",
                  "lastError": null
                }
              }
            }
            """);

        DrillReportSummary? summary = ResilienceManagementService.ReadDrillRunReport(raw);

        Assert.NotNull(summary);
        var report = ResilienceManagementService.MapDrillRunReport(summary);
        Assert.Equal("Succeeded", report.GenerationStatus);
        Assert.Equal("Finalized", report.FinalizationState);
        Assert.Equal("Html", Assert.Single(report.AvailableFormats));
        Assert.Equal(DateTimeOffset.Parse("2026-10-20T11:32:33Z"), report.LastGeneratedOn);
        Assert.Equal("Failover", Assert.Single(report.StageStatuses).DrillRunStage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("""{"properties":{"jobType":"DrillRun"}}""")]
    [InlineData("""{"properties":{"report":null}}""")]
    [InlineData("""{"id":"/x"}""")]
    public void ReadDrillRunReport_ReturnsNullWhenNoReportIsPresent(string? content)
    {
        BinaryData? raw = content is null ? null : BinaryData.FromString(content);

        Assert.Null(ResilienceManagementService.ReadDrillRunReport(raw));
    }

    [Fact]
    public void MapDrillRunReportDownload_MapsUrlFormatAndExpiry()
    {
        var expires = DateTimeOffset.Parse("2026-10-22T02:00:00Z");
        ListReportDownloadUrlResult result = ArmResilienceManagementModelFactory.ListReportDownloadUrlResult(
            DrillReportFormat.Html, new Uri("https://example.invalid/report.html?sig=abc"), expires);

        var info = ResilienceManagementService.MapDrillRunReportDownload(result, null);

        Assert.Equal("Html", info.Format);
        Assert.Equal("https://example.invalid/report.html?sig=abc", info.DownloadUri);
        Assert.Equal(expires, info.ExpiresOn);
    }

    [Fact]
    public void MapDrillRunReportDownload_FallsBackToRequestedFormat()
    {
        ListReportDownloadUrlResult result = ArmResilienceManagementModelFactory.ListReportDownloadUrlResult(
            downloadUri: new Uri("https://example.invalid/report.html"));

        var info = ResilienceManagementService.MapDrillRunReportDownload(result, "Html");

        Assert.Equal("Html", info.Format);
        Assert.Null(info.ExpiresOn);
    }

    [Fact]
    public void MapDrillRunReportDownload_ReadsServiceWireShapeWhenModelIsEmpty()
    {
        ListReportDownloadUrlResult result = ArmResilienceManagementModelFactory.ListReportDownloadUrlResult();
        var raw = BinaryData.FromString("""
            {"status":"Succeeded","properties":{"format":"Html","downloadUrl":"https://example.invalid/report.html?sig=abc","expiryTimestamp":"2026-10-22T16:30:19Z"}}
            """);

        var info = ResilienceManagementService.MapDrillRunReportDownload(result, null, raw);

        Assert.Equal("Html", info.Format);
        Assert.Equal("https://example.invalid/report.html?sig=abc", info.DownloadUri);
        Assert.Equal(DateTimeOffset.Parse("2026-10-22T16:30:19Z"), info.ExpiresOn);
    }

    [Fact]
    public void MapDrillRunReportDownload_ThrowsWhenServiceReturnsNoUrl()
    {
        ListReportDownloadUrlResult result = ArmResilienceManagementModelFactory.ListReportDownloadUrlResult(DrillReportFormat.Html);

        Assert.Throws<InvalidOperationException>(() => ResilienceManagementService.MapDrillRunReportDownload(result, null));
    }
}
