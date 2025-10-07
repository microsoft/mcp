// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Options;

/// <summary>
/// Option definitions for Azure SignalR commands.
/// </summary>
public static class SignalROptionDefinitions
{
    public const string SignalRName = "signalr";

    /// <summary>
    /// SignalR service name option.
    /// </summary>
    public static readonly Option<string> SignalR = new($"--{SignalRName}")
    {
        Description = "The name of the SignalR runtime"
    };
}
