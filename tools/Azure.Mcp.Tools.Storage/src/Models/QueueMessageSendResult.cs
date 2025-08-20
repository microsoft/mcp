// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

public record QueueMessageSendResult
{
    [JsonPropertyName("messageId")]
    public required string MessageId { get; init; }

    [JsonPropertyName("insertionTime")]
    public required DateTimeOffset InsertionTime { get; init; }

    [JsonPropertyName("expirationTime")]
    public required DateTimeOffset ExpirationTime { get; init; }

    [JsonPropertyName("popReceipt")]
    public required string PopReceipt { get; init; }

    [JsonPropertyName("nextVisibleTime")]
    public required DateTimeOffset NextVisibleTime { get; init; }

    [JsonPropertyName("messageContent")]
    public required string MessageContent { get; init; }
}
