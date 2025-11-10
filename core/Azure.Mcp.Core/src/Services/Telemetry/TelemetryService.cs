// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Core.Areas.Server;
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
    private readonly IMachineInformationProvider _informationProvider;
    private readonly bool _isEnabled;
    private readonly ILogger<TelemetryService> _logger;
    private readonly List<KeyValuePair<string, object?>> _tagsList;
    private readonly SemaphoreSlim _initalizeLock = new(1);

    /// <summary>
    /// Task created on the first invocation of <see cref="InitializeAsync"/>.
    /// This is saved so that repeated invocations will see the same exception
    /// as the first invocation.
    /// </summary>
    private Task? _initalizationTask = null;

    private bool _initializationSuccessful;
    private bool _isInitialized;

    internal ActivitySource Parent { get; }

    public TelemetryService(IMachineInformationProvider informationProvider,
        IOptions<AzureMcpServerConfiguration> options,
        IOptions<ServiceStartOptions>? serverOptions,
        ILogger<TelemetryService> logger)
    {
        _isEnabled = options.Value.IsTelemetryEnabled;
        _tagsList =
        [
            new(TagName.AzureMcpVersion, options.Value.Version),
        ];

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
        if (!_isEnabled)
        {
            return [];
        }

        CheckInitialization();
        return [.. _tagsList];
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Activity? StartActivity(string activityName) => StartActivity(activityName, null);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Activity? StartActivity(string activityName, Implementation? clientInfo)
    {
        if (!_isEnabled)
        {
            return null;
        }

        CheckInitialization();

        var activity = Parent.StartActivity(activityName);

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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Activity? StartActivity<T>(string activityName, RequestContext<T> request) where T : RequestParams
    {
        if (!_isEnabled)
        {
            return null;
        }

        CheckInitialization();

        var activity = Parent.StartActivity(activityName);

        if (activity == null)
        {
            return activity;
        }

        var clientInfo = request.Server.ClientInfo;
        if (clientInfo != null)
        {
            activity.AddTag(TagName.ClientName, clientInfo.Name)
                .AddTag(TagName.ClientVersion, clientInfo.Version);
        }

        var userIdentity = request.User?.Identity;
        if (userIdentity != null)
        {
            activity.AddTag("UserName", userIdentity.Name);
        }

        var serverInfo = request.Server.ServerOptions.ServerInfo;
        if (serverInfo != null)
        {
            activity.AddTag("ServerName", serverInfo.Name)
                .AddTag("ServerVersion", serverInfo.Version);
        }

        activity.AddTag(TagName.EventId, Guid.NewGuid().ToString());

        _tagsList.ForEach(kvp => activity.AddTag(kvp.Key, kvp.Value));

        var requestParams = request.Params;
        if (requestParams != null)
        {
            activity.AddTag("_meta", requestParams.Meta?.ToString() ?? "none");
        }
        else
        {
            activity.AddTag("_meta", "noequestparams");
        }

        activity.AddTag("Items", JsonSerializer.Serialize(request.Items, ServerJsonContext.Default.DictionaryStringObject));
        activity.AddTag("Method", request.JsonRpcRequest.Method);

        return activity;
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!_isEnabled)
        {
            return;
        }

        // Quick check if initialization already happened. Avoids
        // trying to get the lock.
        if (_initalizationTask == null)
        {
            // Get async lock for starting initialization
            await _initalizeLock.WaitAsync();

            try
            {
                // Check after acquiring lock to ensure we honor work
                // started while we were waiting.
                if (_initalizationTask == null)
                {
                    _initalizationTask = InnerInitializeAsync();
                }
            }
            finally
            {
                _initalizeLock.Release();
            }
        }

        // Await the response of the initialization work regardless of if
        // we or another invocation created the Task representing it. All
        // awaiting on this will give the same result to ensure idempotency.
        await _initalizationTask;

        async Task InnerInitializeAsync()
        {
            try
            {
                var macAddressHash = await _informationProvider.GetMacAddressHash();
                var deviceId = await _informationProvider.GetOrCreateDeviceId();

                _tagsList.Add(new(TagName.MacAddressHash, macAddressHash));
                _tagsList.Add(new(TagName.DevDeviceId, deviceId));

                _initializationSuccessful = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred initializing telemetry service.");
                throw;
            }
            finally
            {
                _isInitialized = true;
            }
        }
    }

    private void CheckInitialization()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                $"Telemetry service has not been initialized. Use {nameof(InitializeAsync)}() before any other operations.");
        }

        if (!_initializationSuccessful)
        {
            throw new InvalidOperationException("Telemetry service was not successfully initialized. Check logs for initialization errors.");
        }

    }
}
