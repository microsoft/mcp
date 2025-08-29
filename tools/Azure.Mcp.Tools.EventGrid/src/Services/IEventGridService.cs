// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.EventGrid;
using Azure.Mcp.Core.Services;
using Azure.Mcp.Tools.EventGrid.Models;

namespace Azure.Mcp.Tools.EventGrid.Services;

public interface IEventGridService
{
    Task<List<EventGridTopicInfo>> GetTopicsAsync(
        string subscription,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null);
}
