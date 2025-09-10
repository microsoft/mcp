// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Quota.Commands.Region;
using Azure.Mcp.Tools.Quota.Commands.Usage;
using Azure.Mcp.Tools.Quota.Services.Util;

namespace Azure.Mcp.Tools.Quota.Commands;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(CheckCommand.UsageCheckCommandResult))]
[JsonSerializable(typeof(AvailabilityListCommand.RegionCheckCommandResult))]
[JsonSerializable(typeof(UsageInfo))]
[JsonSerializable(typeof(Dictionary<string, List<UsageInfo>>))]
internal sealed partial class QuotaJsonContext : JsonSerializerContext
{
}
