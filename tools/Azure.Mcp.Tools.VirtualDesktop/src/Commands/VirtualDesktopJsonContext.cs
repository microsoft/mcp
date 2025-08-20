// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands;

[JsonSerializable(typeof(HostpoolListCommand.HostPoolListCommandResult))]
[JsonSerializable(typeof(SessionHostListCommand.SessionHostListCommandResult))]
[JsonSerializable(typeof(SessionHostUserSessionListCommand.SessionHostUserSessionListCommandResult))]
[JsonSerializable(typeof(HostPool))]
[JsonSerializable(typeof(Models.SessionHost))]
[JsonSerializable(typeof(UserSession))]
[JsonSerializable(typeof(List<Models.SessionHost>))]
[JsonSerializable(typeof(List<UserSession>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class VirtualDesktopJsonContext : JsonSerializerContext
{
}
