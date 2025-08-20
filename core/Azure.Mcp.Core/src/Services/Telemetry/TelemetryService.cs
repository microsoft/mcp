// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Configuration;
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
    private readonly TaskCompletionSource _isInitialized = new TaskCompletionSource();

    internal ActivitySource Parent { get; }

    public TelemetryService(IMachineInformationProvider informationProvider, IOptions<AzureMcpServerConfiguration> options)
    {
        _isEnabled = options.Value.IsTelemetryEnabled;
        _tagsList = new List<KeyValuePair<string, object?>>()
        {
            new(TagName.AzureMcpVersion, options.Value.Version),
        };

        Parent = new ActivitySource(options.Value.Name, options.Value.Version, _tagsList);
        _informationProvider = informationProvider;

        Task.Factory.StartNew(InitializeTagList);
    }

    public ValueTask<Activity?> StartActivity(string activityId) => StartActivity(activityId, null);

    public async ValueTask<Activity?> StartActivity(string activityId, Implementation? clientInfo)
    {
        if (!_isEnabled)
        {
            return null;
        }

        await _isInitialized.Task;

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

    private async Task InitializeTagList()
    {
        try
        {
            var macAddressHash = await _informationProvider.GetMacAddressHash();
            var deviceId = await _informationProvider.GetOrCreateDeviceId();

            _tagsList.Add(new(TagName.MacAddressHash, macAddressHash));
            _tagsList.Add(new(TagName.DevDeviceId, deviceId));

            _isInitialized.SetResult();
        }
        catch (Exception ex)
        {
            _isInitialized.SetException(ex);
        }
    }
}
