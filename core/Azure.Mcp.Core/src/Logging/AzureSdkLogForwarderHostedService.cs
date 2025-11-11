// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Hosting;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// Hosted service that ensures the Azure SDK EventSource log forwarder is kept alive
/// for the lifetime of the application.
/// </summary>
internal sealed class AzureSdkLogForwarderHostedService : IHostedService
{
    private readonly AzureSdkEventSourceLogForwarder _forwarder;

    public AzureSdkLogForwarderHostedService(AzureSdkEventSourceLogForwarder forwarder)
    {
        _forwarder = forwarder ?? throw new ArgumentNullException(nameof(forwarder));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The forwarder is already listening through its constructor
        // This service just keeps a reference to prevent garbage collection
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Dispose the forwarder when the host shuts down
        _forwarder?.Dispose();
        return Task.CompletedTask;
    }
}
