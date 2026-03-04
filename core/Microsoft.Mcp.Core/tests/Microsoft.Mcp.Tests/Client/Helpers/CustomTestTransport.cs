// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Channels;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Tests.Client.Helpers;

public sealed class CustomTestTransport : ITransport
{
    private readonly Channel<JsonRpcMessage> _messageChannel;

    public bool IsConnected { get; set; }

    public ChannelReader<JsonRpcMessage> MessageReader => _messageChannel;

    public List<JsonRpcMessage> SentMessages { get; } = [];

    public Action<JsonRpcMessage>? MessageListener { get; set; }

    public string? SessionId => null;

    public CustomTestTransport()
    {
        _messageChannel = Channel.CreateUnbounded<JsonRpcMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });
        IsConnected = true;
    }

    public ValueTask DisposeAsync()
    {
        _messageChannel.Writer.TryComplete();
        IsConnected = false;
        return ValueTask.CompletedTask;
    }

    public async Task SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        SentMessages.Add(message);
        if (message is JsonRpcRequest request)
        {
            await WriteMessageAsync(request, cancellationToken);
            MessageListener?.Invoke(message);
        }
        else if (message is JsonRpcNotification notification)
        {
            await WriteMessageAsync(notification, cancellationToken);
            MessageListener?.Invoke(message);
        }
        else if (message is JsonRpcResponse response)
        {
            MessageListener?.Invoke(message);
        }
        else
        {
            throw new NotSupportedException($"Message type {message.GetType()} is not supported.");
        }
    }

    private async Task WriteMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        await _messageChannel.Writer.WriteAsync(message, cancellationToken);
    }
}
