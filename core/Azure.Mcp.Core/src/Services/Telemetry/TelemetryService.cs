// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Diagnostics;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Services.Telemetry;

/// <summary>
/// Provides access to services.
/// </summary>
internal class TelemetryService : ITelemetryService
{
    private readonly bool _isEnabled;
    private readonly List<KeyValuePair<string, object?>> _tagsList;
    private readonly IMachineInformationProvider _informationProvider;
    private readonly ILogger<TelemetryService> _logger;
    private bool _isInitialized;

    internal ActivitySource Parent { get; }

    public TelemetryService(IMachineInformationProvider informationProvider,
        IOptions<AzureMcpServerConfiguration> options,
        IOptions<ServiceStartOptions>? serverOptions, 
        ILogger<TelemetryService> logger)
    {
        _isEnabled = options.Value.IsTelemetryEnabled;
        _tagsList = new List<KeyValuePair<string, object?>>()
        {
            new(TagName.AzureMcpVersion, options.Value.Version),
        };

        if (serverOptions?.Value != null)
        {
            _tagsList.Add(new(TagName.ServerMode, serverOptions.Value.Mode));
        }

        Parent = new ActivitySource(options.Value.Name, options.Value.Version, _tagsList);
        _informationProvider = informationProvider;
        _logger = logger;
    }

    /// <summary>
    /// TESTING PURPOSES ONLY: Gets the default tags used for telemetry.
    /// </summary>
    internal IReadOnlyList<KeyValuePair<string, object?>> GetDefaultTags()
    {
        return _tagsList.ToImmutableList();
    }

    public Activity? StartActivity(string activityId) => StartActivity(activityId, null);

    public Activity? StartActivity(string activityId, Implementation? clientInfo)
    {
        if (!_isEnabled)
        {
            return null;
        }

        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                "Telemetry service has not been initialized. Use InitializeAsync() before any other operations.");
        }

        var activity = Parent.StartActivity(activityId);

        if (activity == null)
        {
            return activity;
        }

        if (clientInfo != null)
        {
            activity.AddTag(TagName.ClientName, clientInfo.Name)
                .AddTag(TagName.ClientVersion, clientInfo.Version);
        }

        activity.AddTag(TagName.EventId, Guid.NewGuid().ToString());

        _tagsList.ForEach(kvp => activity.AddTag(kvp.Key, kvp.Value));

        return activity;
    }

    public void Dispose()
    {
    }

    public async ValueTask InitializeAsync()
    {
        var wasInitialized = Interlocked.CompareExchange(ref _isInitialized, true, false);

        if (wasInitialized)
        {
            return;
        }

        try
        {
            var macAddressHash = await _informationProvider.GetMacAddressHash();
            var deviceId = await _informationProvider.GetOrCreateDeviceId();

            _tagsList.Add(new(TagName.MacAddressHash, macAddressHash));
            _tagsList.Add(new(TagName.DevDeviceId, deviceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred initializing telemetry service.");
            throw;
        }
    }
}
