// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Insights.Commands;

[JsonSerializable(typeof(InsightsGetCommand.InsightsGetCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class InsightsJsonContext : JsonSerializerContext
{
}
