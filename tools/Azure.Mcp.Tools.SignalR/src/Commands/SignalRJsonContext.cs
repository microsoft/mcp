// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SignalR.Commands.Identity;
using Azure.Mcp.Tools.SignalR.Commands.Key;
using Azure.Mcp.Tools.SignalR.Commands.NetworkRule;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;

namespace Azure.Mcp.Tools.SignalR.Commands;

/// <summary>
/// JSON serialization context for Azure SignalR Service commands.
/// </summary>
[JsonSerializable(typeof(IdentityListCommand.IdentityListCommandResult))]
[JsonSerializable(typeof(KeyListCommand.KeyListCommandResult))]
[JsonSerializable(typeof(NetworkRuleListCommand.NetworkRuleListCommandResult))]
[JsonSerializable(typeof(RuntimeListCommand.RuntimeListCommandResult))]
[JsonSerializable(typeof(RuntimeShowCommand.RuntimeShowCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SignalRJsonContext : JsonSerializerContext;
