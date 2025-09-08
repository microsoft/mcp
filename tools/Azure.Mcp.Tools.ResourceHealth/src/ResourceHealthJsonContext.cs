// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Models;
using Azure.Mcp.Tools.ResourceHealth.Models.Internal;

namespace Azure.Mcp.Tools.ResourceHealth;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(AvailabilityStatus))]
[JsonSerializable(typeof(ServiceHealthEvent))]
[JsonSerializable(typeof(List<AvailabilityStatus>))]
[JsonSerializable(typeof(List<ServiceHealthEvent>))]
[JsonSerializable(typeof(AvailabilityStatusResponse))]
[JsonSerializable(typeof(AvailabilityStatusListResponse))]
[JsonSerializable(typeof(ServiceHealthEventResponse))]
[JsonSerializable(typeof(ServiceHealthEventListResponse))]
[JsonSerializable(typeof(AvailabilityStatusGetCommand.AvailabilityStatusGetCommandResult))]
[JsonSerializable(typeof(AvailabilityStatusListCommand.AvailabilityStatusListCommandResult))]
[JsonSerializable(typeof(ServiceHealthEventsListCommand.ServiceHealthEventsListCommandResult))]
internal partial class ResourceHealthJsonContext : JsonSerializerContext;
