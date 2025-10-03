// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Communication.Models;

namespace Azure.Mcp.Tools.Communication;

[JsonSerializable(typeof(Azure.Mcp.Tools.Communication.Models.SmsResult))]
[JsonSerializable(typeof(Azure.Mcp.Tools.Communication.Models.SmsSendCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class CommunicationJsonContext : JsonSerializerContext
{
}
