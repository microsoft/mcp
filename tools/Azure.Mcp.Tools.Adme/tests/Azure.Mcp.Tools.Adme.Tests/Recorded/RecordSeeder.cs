// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Seeds and purges records once for the storage recorded test class.</summary>
public sealed class RecordSeeder : IAsyncLifetime
{
    private const string FixtureFile = "TestData/well-records.json";
    private const string MarkerPlaceholder = "MARKER";
    private const string MarkerPrefix = "mcptest";
    private const string PlaybackMarker = "recording";
    private const string PlaybackPartition = "recording-partition";
    private const int RecordCount = 3;
    private const string TimestampFormat = "yyyyMMddHHmmss";
    private const string WellKind = "osdu:wks:master-data--Well:1.0.0";

    private ServiceProvider? _provider;
    private IStorageService? _storageService;
    private string? _endpoint;
    private string? _tenant;

    public string DataPartition { get; private set; } = PlaybackPartition;

    public string Marker { get; private set; } = PlaybackMarker;

    public IReadOnlyList<string> Ids { get; private set; } = CreateIds(PlaybackPartition, PlaybackMarker, RecordCount);

    public string FirstId => Ids[0];

    public async ValueTask InitializeAsync()
    {
        if (!LiveTestSettings.TryLoadTestSettings(out var settings)
            || settings.TestMode == TestMode.Playback)
        {
            return;
        }

        _endpoint = GetRequiredSetting(settings, "ADME_MCP_SERVER_URL");
        DataPartition = GetRequiredSetting(settings, "ADME_MCP_SERVER_DATA_PARTITION");
        var legalTag = GetRequiredSetting(settings, "ADME_MCP_SERVER_LEGAL_TAG");
        _tenant = string.IsNullOrWhiteSpace(settings.TenantId) ? null : settings.TenantId;
        Marker = MarkerPrefix
            + DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture)
            + "x" + Guid.NewGuid().ToString("N")[..6];

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build());
        services.AddAzureService();
        new AdmeSetup().ConfigureServices(services);
        _provider = services.BuildServiceProvider();
        _storageService = _provider.GetRequiredService<IStorageService>();

        var data = ReadFixture(Marker);
        var records = CreateRecords(DataPartition, legalTag, data);
        var response = await _storageService.UpsertRecordsAsync(
            _endpoint, DataPartition, records, _tenant, CancellationToken.None);
        Ids = response.RecordIds
            ?? throw new InvalidOperationException("ADME storage seeding returned no record IDs.");
        if (Ids.Count != records.Length)
        {
            throw new InvalidOperationException(
            $"ADME storage seeding returned {Ids.Count} record IDs; expected {records.Length}.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_storageService is not null && _endpoint is not null)
        {
            foreach (var id in Ids)
            {
                try
                {
                    await _storageService.DeleteRecordAsync(
                        _endpoint, DataPartition, id, _tenant, CancellationToken.None);
                }
                catch
                {
                    // Best-effort cleanup must not strand the remaining seeded records.
                }
            }
        }

        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }
    }

    private static JsonElement ReadFixture(string marker)
    {
        var path = Path.Combine(AppContext.BaseDirectory, FixtureFile);
        var json = File.ReadAllText(path).Replace(MarkerPlaceholder, marker, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(
            json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        return document.RootElement.Clone();
    }

    private static StorageRecord[] CreateRecords(
        string dataPartition,
        string legalTag,
        JsonElement data)
    {
        var ids = CreateIds(dataPartition, data.GetProperty("FacilityID").GetString()!, RecordCount);
        return Enumerable.Range(0, RecordCount).Select(index => new StorageRecord
        {
            Id = ids[index],
            Kind = WellKind,
            Acl = new RecordAcl
            {
                Viewers = [$"data.default.viewers@{dataPartition}.dataservices.energy"],
                Owners = [$"data.default.owners@{dataPartition}.dataservices.energy"],
            },
            Legal = new RecordLegal
            {
                LegalTags = [legalTag],
                OtherRelevantDataCountries = ["US"],
            },
            Data = data,
        }).ToArray();
    }

    private static string[] CreateIds(string dataPartition, string marker, int count) =>
        Enumerable.Range(0, count)
            .Select(index => $"{dataPartition}:master-data--Well:{marker}-{index}")
            .ToArray();

    private static string GetRequiredSetting(LiveTestSettings settings, string name)
    {
        var value = settings.EnvironmentVariables.GetValueOrDefault(name)
            ?? Environment.GetEnvironmentVariable(name);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{name} must be configured for live or record mode.");
    }
}
