// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

/// <summary>
/// Serves simple HTTP responses on loopback for transport tests without external network access.
/// </summary>
/// <param name="listener">The listener owned and started by this fixture.</param>
/// <param name="redirect">Whether the first response redirects to a second path.</param>
internal sealed class LoopbackHttpServer(TcpListener listener, bool redirect = false) : IAsyncDisposable
{
    private readonly (CancellationTokenSource Lifetime, ConcurrentQueue<string> Requests, Task Run) _state = StartListener(listener, redirect);

    /// <summary>
    /// Gets the dynamically allocated loopback HTTP endpoint.
    /// </summary>
    public Uri Endpoint => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");

    /// <summary>
    /// Gets a snapshot of the received request lines and headers.
    /// </summary>
    public IReadOnlyList<string> Requests => _state.Requests.ToArray();

    /// <summary>
    /// Starts an isolated <see cref="TcpListener"/> on an available loopback port.
    /// </summary>
    /// <param name="redirect">Whether to return a redirect before the successful response.</param>
    /// <returns>
    /// A fixture that must be asynchronously disposed after its clients.
    /// </returns>
    public static LoopbackHttpServer Start(bool redirect = false) => new(new TcpListener(IPAddress.Loopback, 0), redirect);

    /// <summary>
    /// Stops the <see cref="TcpListener"/> and awaits its request-processing loop.
    /// </summary>
    /// <returns>
    /// The <see cref="ValueTask"/> representing completion of fixture cleanup.
    /// </returns>
    public async ValueTask DisposeAsync()
    {
        await _state.Lifetime.CancelAsync();
        listener.Stop();
        await _state.Run;
        _state.Lifetime.Dispose();
    }

    private static (CancellationTokenSource, ConcurrentQueue<string>, Task) StartListener(TcpListener listener, bool redirect)
    {
        listener.Start();
        var lifetime = new CancellationTokenSource();
        var requests = new ConcurrentQueue<string>();
        return (lifetime, requests, ServeAsync(listener, requests, redirect, lifetime.Token));
    }

    private static async Task ServeAsync(TcpListener listener, ConcurrentQueue<string> requests, bool redirect, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                using TcpClient connection = await listener.AcceptTcpClientAsync(cancellationToken);
                await using NetworkStream stream = connection.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                var headers = new StringBuilder();
                while (await reader.ReadLineAsync(cancellationToken) is string line && line.Length > 0)
                {
                    headers.AppendLine(line);
                }

                requests.Enqueue(headers.ToString());
                string responseText = redirect && requests.Count == 1
                    ? "HTTP/1.1 302 Found\r\nLocation: /second\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                    : "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\n{}";
                byte[] response = Encoding.ASCII.GetBytes(responseText);
                await stream.WriteAsync(response, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
            // Stopping the listener can dispose its socket before the pending accept observes cancellation.
        }
        catch (InvalidOperationException) when (cancellationToken.IsCancellationRequested)
        {
            // On macOS, stopping the listener can report that it is no longer listening before accept observes cancellation.
        }
    }
}
