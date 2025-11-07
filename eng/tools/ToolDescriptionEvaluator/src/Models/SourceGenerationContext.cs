// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolSelection.Models;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ListToolsResult))]
[JsonSerializable(typeof(List<Tool>))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(Dictionary<string, List<string>>), TypeInfoPropertyName = "DictionaryStringListString")]
[JsonSerializable(typeof(EmbeddingRequest))]
[JsonSerializable(typeof(EmbeddingResponse))]
[JsonSerializable(typeof(EmbeddingData))]
[JsonSerializable(typeof(Usage))]
[JsonSerializable(typeof(ApiError))]
[JsonSerializable(typeof(SuccessRateMetrics))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
