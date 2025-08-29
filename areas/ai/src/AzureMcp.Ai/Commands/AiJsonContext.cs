// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Ai.Commands.OpenAi;
using AzureMcp.Ai.Models;

namespace AzureMcp.Ai.Commands;

[JsonSerializable(typeof(OpenAiCompletionsCreateCommand.OpenAiCompletionsCreateCommandResult), TypeInfoPropertyName = "OpenAiCompletionsCreateCommandResult")]
[JsonSerializable(typeof(CompletionResult), TypeInfoPropertyName = "CompletionResult")]
internal partial class AiJsonContext : JsonSerializerContext
{
}
