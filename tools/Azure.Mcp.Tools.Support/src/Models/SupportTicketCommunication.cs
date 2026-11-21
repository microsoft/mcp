// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Support.Models;

public sealed record SupportTicketCommunication(
    string? Name,
    string? Subject,
    string? Body,
    string? Sender,
    string? CommunicationType,
    string? CommunicationDirection,
    DateTimeOffset? CreatedOn);
