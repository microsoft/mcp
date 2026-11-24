// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Services;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Assessment;

public class AssessmentCommandTests
{
    private const string SasUri = "https://sa.blob.core.windows.net/zips/collector.zip?sp=r&sig=secret";
    private const string OutputUri = "https://sa.blob.core.windows.net/out/recommendations.json?sp=r&sig=out";
    private const string HaWriteSas = "https://sa.blob.core.windows.net/c/ha-1/input?sp=cw&sig=write";

    private sealed class SequenceHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private int _calls;

        public List<HttpRequestMessage> Requests { get; } = [];
        public List<byte[]> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null ? [] : await request.Content.ReadAsByteArrayAsync(cancellationToken));
            HttpStatusCode status = statuses[Math.Min(_calls++, statuses.Length - 1)];
            return new HttpResponseMessage(status) { Content = new StringContent(status == HttpStatusCode.Created ? string.Empty : "error") };
        }
    }

    [Theory]
    [InlineData("assess-1", true)]
    [InlineData("A_b-9", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("-starts-with-dash", false)]
    [InlineData("has space", false)]
    public void IsValidAssessmentName(string? name, bool expected) =>
        Assert.Equal(expected, AssessmentValidation.IsValidAssessmentName(name));

    [Fact]
    public void IsValidAssessmentName_RejectsMoreThan64Characters()
    {
        Assert.True(AssessmentValidation.IsValidAssessmentName(new string('a', 64)));
        Assert.False(AssessmentValidation.IsValidAssessmentName(new string('a', 65)));
    }

    [Theory]
    [InlineData("eastus", "eastus")]
    [InlineData(" East US ", "eastus")]
    [InlineData("WestEurope", "westeurope")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("east-us", null)]
    [InlineData("1eastus", null)]
    public void NormalizeRegion(string? region, string? expected) =>
        Assert.Equal(expected, AssessmentValidation.NormalizeRegion(region));

    [Theory]
    [InlineData(SasUri, true)]
    [InlineData("http://127.0.0.1:10000/devstoreaccount1/zips/c.zip?sig=x", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("collector.zip", false)]
    [InlineData("ftp://host/collector.zip", false)]
    public void IsValidInventorySasUri(string? uri, bool expected) =>
        Assert.Equal(expected, AssessmentValidation.IsValidInventorySasUri(uri));

    [Fact]
    public void BuildSettings_NoOptionalSettings_ReturnsNullSoDefaultsApply()
    {
        var (settings, error) = AssessmentValidation.BuildSettings(new CreateAssessmentOptions { DiskTypes = [], VmFamilies = [" "] });

        Assert.Null(error);
        Assert.Null(settings);
    }

    [Fact]
    public void BuildSettings_NormalizesEnumCasing()
    {
        var (settings, error) = AssessmentValidation.BuildSettings(new CreateAssessmentOptions
        {
            SavingsOption = "ri3year",
            AzureHybridBenefit = "yes",
            DiskTypes = ["premium", "PREMIUMV2"],
            EnvironmentType = " devtest ",
        });

        Assert.Null(error);
        Assert.Equal("RI3Year", settings!.SavingsOption);
        Assert.Equal("Yes", settings.AzureHybridBenefit);
        Assert.Equal(["Premium", "PremiumV2"], settings.DiskTypes!);
        Assert.Equal("DevTest", settings.EnvironmentType);
    }

    [Fact]
    public void BuildSettings_RejectsInvalidValues()
    {
        var (settings, error) = AssessmentValidation.BuildSettings(new CreateAssessmentOptions
        {
            ScalingFactor = 3,
            UptimeHoursPerDay = 25,
            SavingsOption = "RI5Year",
            DiskTypes = ["Premium", "Floppy"],
            BillingSubscriptionId = "not-a-guid",
        });

        Assert.Null(settings);
        Assert.Contains("'scaling-factor'", error);
        Assert.Contains("'uptime-hours-per-day'", error);
        Assert.Contains("'savings-option'", error);
        Assert.Contains("'disk-types'", error);
        Assert.Contains("'billing-subscription-id'", error);
    }

    [Fact]
    public void WriteSettings_WritesOnlySuppliedSettingsAndCompletesUptime()
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            AssessmentService.WriteSettings(writer, new AssessmentSettings
            {
                SavingsOption = "RI1Year",
                UptimeHoursPerDay = 8,
                DiskTypes = ["Premium"],
                ManagementServices = ["Backup"],
            });
            writer.WriteEndObject();
        }

        using JsonDocument doc = JsonDocument.Parse(stream.ToArray());
        JsonElement root = doc.RootElement;
        Assert.Equal("RI1Year", root.GetProperty("savingsSettings").GetProperty("savingsOptions").GetString());
        Assert.False(root.GetProperty("savingsSettings").TryGetProperty("azureOfferCode", out _));
        Assert.Equal(31, root.GetProperty("vmUptime").GetProperty("daysPerMonth").GetInt32());
        Assert.Equal(8, root.GetProperty("vmUptime").GetProperty("hoursPerDay").GetInt32());
        Assert.Equal("Premium", root.GetProperty("azureDiskTypes")[0].GetString());
        Assert.Equal("Backup", root.GetProperty("azureManagementServices")[0].GetProperty("serviceName").GetString());
        Assert.False(root.TryGetProperty("billingSettings", out _));
        Assert.False(root.TryGetProperty("scalingFactor", out _));
    }

    [Fact]
    public void AssessmentRequest_ToStringDoesNotContainSasUri() =>
        Assert.DoesNotContain("sig=secret", new AssessmentRequest("assess-1", null, SasUri, "eastus", "USD").ToString());

    [Fact]
    public void HaInputPackage_BuildsZipWithManifestOnly()
    {
        byte[] zip = HaInputPackage.Build(new HaInput(SasUri, OutputUri));

        using ZipArchive archive = new(new MemoryStream(zip), ZipArchiveMode.Read);
        ZipArchiveEntry entry = Assert.Single(archive.Entries);
        Assert.Equal("manifest.json", entry.FullName);
        using JsonDocument manifest = JsonDocument.Parse(entry.Open());
        Assert.Equal(SasUri, manifest.RootElement.GetProperty("inventorySasUri").GetString());
        Assert.Equal(OutputUri, manifest.RootElement.GetProperty("assessmentSasUri").GetString());
    }

    [Fact]
    public async Task HaInputPackage_UploadsBlockBlobWithoutAuthorization()
    {
        var handler = new SequenceHandler(HttpStatusCode.Created);
        byte[] zip = HaInputPackage.Build(new HaInput(SasUri, OutputUri));

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, zip, NullLogger.Instance, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.Equal(201, status);
        Assert.Null(error);
        HttpRequestMessage request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal(HaWriteSas, request.RequestUri!.ToString());
        Assert.Equal("BlockBlob", request.Headers.GetValues("x-ms-blob-type").Single());
        Assert.Null(request.Headers.Authorization);
        Assert.Equal(zip, handler.Bodies[0]);
    }

    [Fact]
    public async Task HaInputPackage_RetriesTransientErrors()
    {
        var handler = new SequenceHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.TooManyRequests, HttpStatusCode.Created);
        var delays = new List<TimeSpan>();

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, [1], NullLogger.Instance,
            (delay, _) => { delays.Add(delay); return Task.CompletedTask; }, TestContext.Current.CancellationToken);

        Assert.Equal(201, status);
        Assert.Null(error);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)], delays);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, 1)]
    [InlineData(HttpStatusCode.InternalServerError, 3)]
    public async Task HaInputPackage_FailsAfterNonTransientErrorOrLastAttempt(HttpStatusCode failure, int expectedAttempts)
    {
        var handler = new SequenceHandler(failure);

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, [1], NullLogger.Instance, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.Equal((int)failure, status);
        Assert.Contains($"HTTP {(int)failure}", error);
        Assert.DoesNotContain("sig=write", error);
        Assert.Equal(expectedAttempts, handler.Requests.Count);
    }
}
