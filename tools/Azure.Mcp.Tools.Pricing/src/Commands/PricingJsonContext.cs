// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Pricing.Models;

namespace Azure.Mcp.Tools.Pricing.Commands;

[JsonSerializable(typeof(PriceItem))]
[JsonSerializable(typeof(PricingGetCommand.PricingGetCommandResult))]
[JsonSerializable(typeof(SavingsPlanPrice))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class PricingJsonContext : JsonSerializerContext;
