// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Queue.Message;

public class QueueMessageSendOptions : BaseQueueOptions
{
    [JsonPropertyName(StorageOptionDefinitions.MessageContent)]
    public string? MessageContent { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.TimeToLiveInSeconds)]
    public int? TimeToLiveInSeconds { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.VisibilityTimeoutInSeconds)]
    public int? VisibilityTimeoutInSeconds { get; set; }
}
