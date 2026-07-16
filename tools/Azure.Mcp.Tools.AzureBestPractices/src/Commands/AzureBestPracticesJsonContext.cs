// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBestPractices.Commands;

[JsonSerializable(typeof(AIAppBestPracticesCommand.AIAppBestPracticesCommandResult))]
[JsonSerializable(typeof(BestPracticesCommand.BestPracticesCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AzureBestPracticesJsonContext : JsonSerializerContext
{

}
