// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Models;

namespace Azure.Mcp.Tools.Communication;

[JsonSerializable(typeof(SmsResult))]
[JsonSerializable(typeof(SmsSendCommandResult))]
[JsonSerializable(typeof(EmailSendCommand.EmailSendCommandResult))]
[JsonSerializable(typeof(EmailSendResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class CommunicationJsonContext : JsonSerializerContext
{
}
