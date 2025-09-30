// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands;

[JsonSerializable(typeof(HostpoolListCommand.HostPoolListCommandResult))]
[JsonSerializable(typeof(SessionHostListCommand.SessionHostListCommandResult))]
[JsonSerializable(typeof(SessionHostUserSessionListCommand.SessionHostUserSessionListCommandResult))]
[JsonSerializable(typeof(HostPool))]
[JsonSerializable(typeof(Models.SessionHost))]
[JsonSerializable(typeof(UserSession))]
[JsonSerializable(typeof(List<Models.SessionHost>))]
[JsonSerializable(typeof(List<UserSession>))]
[JsonSerializable(typeof(HostPoolData))]
[JsonSerializable(typeof(SessionHostData))]
[JsonSerializable(typeof(UserSessionData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class VirtualDesktopJsonContext : JsonSerializerContext
{
}
