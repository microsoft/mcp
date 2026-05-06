// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;

namespace Fabric.Mcp.Tools.DataFactory.Services;

/// <summary>
/// No-op implementation of IUserNotificationService for the Fabric MCP server integration.
/// The Fabric MCP server handles notifications through its own infrastructure.
/// </summary>
internal sealed class NullUserNotificationService : IUserNotificationService
{
    public Task NotifyAsync(string title, string message, NotificationLevel level = NotificationLevel.Info)
    {
        return Task.CompletedTask;
    }
}
