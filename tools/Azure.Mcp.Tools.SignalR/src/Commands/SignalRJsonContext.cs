// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;

namespace Azure.Mcp.Tools.SignalR.Commands;

/// <summary>
/// JSON serialization context for Azure SignalR Service commands.
/// </summary>
[JsonSerializable(typeof(Models.Identity))]
[JsonSerializable(typeof(Models.NetworkAcls))]
[JsonSerializable(typeof(Models.Runtime))]
[JsonSerializable(typeof(Models.RuntimeProperties))]
[JsonSerializable(typeof(Models.Sku))]
[JsonSerializable(typeof(Models.UpstreamTemplate))]
[JsonSerializable(typeof(RuntimeGetCommand.RuntimeGetCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SignalRJsonContext : JsonSerializerContext;
