// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore DRILLSSERVICEGROUPNAME DRILLSDRILLNAME DRILLSDRILLRUNNAME

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

public partial class ResilienceManagementCommandTests
{
    [Fact]
    public async Task Should_generate_drill_run_report()
    {
        var (serviceGroup, drill, drillRun) = GetDrillRunReportFixture();

        var result = await CallToolAsync(
            "resiliency_drill_run_generate-report",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun }
            });

        var operation = result.AssertProperty("result");
        Assert.True(Guid.TryParse(operation.AssertProperty("operationId").GetString(), out _));
        Assert.True(operation.TryGetProperty("hasCompleted", out _));

        // The service replies with the DrillRun envelope, so the summary must be read from properties.report.
        var returnedReport = operation.AssertProperty("report");
        Assert.Equal("Succeeded", returnedReport.AssertProperty("generationStatus").GetString());
        Assert.Contains(
            returnedReport.AssertProperty("availableFormats").EnumerateArray(),
            format => format.GetString() == "Html");

        var report = await WaitForDrillRunReportAsync(serviceGroup, drill, drillRun);
        Assert.Equal("Succeeded", report.AssertProperty("generationStatus").GetString());
        Assert.Contains(
            report.AssertProperty("availableFormats").EnumerateArray(),
            format => format.GetString() == "Html");
    }

    [Fact]
    public async Task Should_get_drill_run_report_download_url()
    {
        var (serviceGroup, drill, drillRun) = GetDrillRunReportFixture();

        // The fixture drill run has already completed all stages, so its report exists.
        await WaitForDrillRunReportAsync(serviceGroup, drill, drillRun);

        // A drill run rejects a second POST with 409 until the previous operation (for example a report
        // generation from another test) releases it.
        JsonElement? download = null;
        for (int attempt = 0; attempt < 90 && download is null; attempt++)
        {
            download = await TryGetReportDownloadWithoutLoggingAsync(serviceGroup, drill, drillRun);
            if (download is null)
            {
                await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
            }
        }

        Assert.NotNull(download);
        Assert.Equal("Html", download.AssertProperty("format").GetString());
        Assert.True(
            Uri.TryCreate(download.AssertProperty("downloadUri").GetString(), UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps,
            "The report download URL must be an absolute https URL.");
        Assert.True(download.Value.TryGetProperty("expiresOn", out _));
    }

    [Fact]
    public async Task Should_reject_unsupported_drill_run_report_download_format()
    {
        var (serviceGroup, drill, drillRun) = GetDrillRunReportFixture();

        JsonElement? response = await CallToolAsync(
            "resiliency_drill_run_get-report-download-url",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "format", "Pdf" }
            },
            resultProcessor: root => root);

        Assert.Equal(400, response.AssertProperty("status").GetInt32());
    }

    // The response carries a signed URL that grants read access to the report. CallToolAsync logs the whole
    // response (to the test output in debug runs, or on failure), so call the client directly and never log it.
    private async Task<JsonElement?> TryGetReportDownloadWithoutLoggingAsync(string serviceGroup, string drill, string drillRun)
    {
        var result = await Client.CallToolAsync(
            "resiliency_drill_run_get-report-download-url",
            new Dictionary<string, object?>
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "format", "html" }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        string? content = McpTestUtilities.GetFirstText(result.Content);
        Assert.False(string.IsNullOrWhiteSpace(content), "The report download URL call returned no content.");

        using var document = JsonDocument.Parse(content!);
        JsonElement root = document.RootElement;
        if (root.TryGetProperty("results", out var results) && results.TryGetProperty("result", out var download))
        {
            return download.Clone();
        }

        // Error responses carry only the sanitized message, so it is safe to include in the failure.
        int status = root.TryGetProperty("status", out var statusElement) ? statusElement.GetInt32() : 0;
        string? message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;
        Assert.True(status == 409, $"Unexpected response while requesting the report download URL: {status} {message}");
        return null;
    }

    private (string ServiceGroup, string Drill, string DrillRun) GetDrillRunReportFixture() => (
        RegisterOrRetrieveDeploymentOutputVariable("serviceGroupName", "DRILLSSERVICEGROUPNAME"),
        RegisterOrRetrieveDeploymentOutputVariable("drillName", "DRILLSDRILLNAME"),
        RegisterOrRetrieveDeploymentOutputVariable("drillRunName", "DRILLSDRILLRUNNAME"));

    private async Task<JsonElement> WaitForDrillRunReportAsync(string serviceGroup, string drill, string drillRun)
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            var result = await CallToolAsync(
                "resiliency_drill_run_get",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "service-group", serviceGroup },
                    { "drill", drill },
                    { "name", drillRun }
                });

            var properties = result.AssertProperty("drillRun").AssertProperty("properties");
            if (properties.TryGetProperty("report", out var report) && report.ValueKind == JsonValueKind.Object &&
                report.TryGetProperty("generationStatus", out var status))
            {
                string? generationStatus = status.GetString();
                Assert.True(generationStatus != "Failed", $"Drill run report generation failed: {report}");
                if (generationStatus == "Succeeded")
                {
                    return report.Clone();
                }
            }

            await Task.Delay(PollInterval(10000), TestContext.Current.CancellationToken);
        }

        Assert.Fail("The drill run report did not reach Succeeded within the timeout.");
        return default;
    }
}
