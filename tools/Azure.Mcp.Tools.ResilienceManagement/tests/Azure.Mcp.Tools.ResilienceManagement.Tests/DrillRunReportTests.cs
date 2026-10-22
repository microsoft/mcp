// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore DRILLSSERVICEGROUPNAME DRILLSDRILLNAME DRILLSDRILLRUNNAME

using System.Text.Json;
using Microsoft.Mcp.Tests;
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

        // The download URL is only available once a report exists, so generate it first.
        await CallToolAsync(
            "resiliency_drill_run_generate-report",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun }
            });
        await WaitForDrillRunReportAsync(serviceGroup, drill, drillRun);

        var result = await CallToolAsync(
            "resiliency_drill_run_get-report-download-url",
            new()
            {
                { "tenant", Settings.TenantId },
                { "service-group", serviceGroup },
                { "drill", drill },
                { "drill-run", drillRun },
                { "format", "html" }
            });

        var download = result.AssertProperty("result");
        Assert.Equal("Html", download.AssertProperty("format").GetString());
        Assert.True(
            Uri.TryCreate(download.AssertProperty("downloadUri").GetString(), UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps,
            "The report download URL must be an absolute https URL.");
        Assert.True(download.TryGetProperty("expiresOn", out _));
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
