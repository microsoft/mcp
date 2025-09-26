// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Communication.Commands.Sms;
using AzureMcp.Communication.Models;

namespace AzureMcp.Communication;

[JsonSerializable(typeof(SmsResult))]
[JsonSerializable(typeof(SmsSendCommand.SmsSendCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class CommunicationJsonContext : JsonSerializerContext
{
}
