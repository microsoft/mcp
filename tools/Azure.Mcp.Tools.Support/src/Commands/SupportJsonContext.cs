// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Support.Commands.ProblemClassification;
using Azure.Mcp.Tools.Support.Commands.Service;
using Azure.Mcp.Tools.Support.Commands.Ticket;
using Azure.Mcp.Tools.Support.Models;

namespace Azure.Mcp.Tools.Support.Commands;

[JsonSerializable(typeof(TicketListCommand.TicketListCommandResult))]
[JsonSerializable(typeof(TicketGetCommand.TicketGetCommandResult))]
[JsonSerializable(typeof(TicketCreateCommand.TicketCreateCommandResult))]
[JsonSerializable(typeof(ServiceListCommand.ServiceListCommandResult))]
[JsonSerializable(typeof(ProblemClassificationListCommand.ProblemClassificationListCommandResult))]
[JsonSerializable(typeof(SupportTicket))]
[JsonSerializable(typeof(SupportTicketCommunication))]
[JsonSerializable(typeof(SupportServiceInfo))]
[JsonSerializable(typeof(SupportProblemClassification))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SupportJsonContext : JsonSerializerContext;
