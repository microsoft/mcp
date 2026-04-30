// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FoundryExtensions.Models;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

[JsonSerializable(typeof(KnowledgeIndexListCommand.KnowledgeIndexListCommandResult))]
[JsonSerializable(typeof(KnowledgeIndexSchemaCommand.KnowledgeIndexSchemaCommandResult))]
[JsonSerializable(typeof(OpenAiCompletionsCreateCommand.OpenAiCompletionsCreateCommandResult))]
[JsonSerializable(typeof(OpenAiEmbeddingsCreateCommand.OpenAiEmbeddingsCreateCommandResult))]
[JsonSerializable(typeof(OpenAiModelsListCommand.OpenAiModelsListCommandResult))]
[JsonSerializable(typeof(OpenAiChatCompletionsCreateCommand.OpenAiChatCompletionsCreateCommandResult))]
[JsonSerializable(typeof(ResourceGetCommand.ResourceGetCommandResult))]
[JsonSerializable(typeof(AiResourceInformation))]
[JsonSerializable(typeof(DeploymentInformation))]
[JsonSerializable(typeof(ChatCompletionCreateResult))]
[JsonSerializable(typeof(ChatCompletionResult))]
[JsonSerializable(typeof(ChatCompletionChoice))]
[JsonSerializable(typeof(ChatCompletionMessage))]
[JsonSerializable(typeof(ChatCompletionUsageInfo))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(KnowledgeIndexInformation))]
[JsonSerializable(typeof(KnowledgeIndexSchema))]
[JsonSerializable(typeof(CompletionResult))]
[JsonSerializable(typeof(CompletionUsageInfo))]
[JsonSerializable(typeof(EmbeddingResult))]
[JsonSerializable(typeof(EmbeddingData))]
[JsonSerializable(typeof(EmbeddingUsageInfo))]
[JsonSerializable(typeof(OpenAiModelsListResult))]
[JsonSerializable(typeof(OpenAiModelDeployment))]
[JsonSerializable(typeof(OpenAiModelCapabilities))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class FoundryExtensionsJsonContext : JsonSerializerContext;
