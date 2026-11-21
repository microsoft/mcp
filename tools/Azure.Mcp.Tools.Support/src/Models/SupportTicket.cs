// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Support.Models;

public sealed record SupportTicket(
    string? Name,
    string? Id,
    string? SupportTicketId,
    string? Title,
    string? Description,
    string? Status,
    string? Severity,
    string? ServiceId,
    string? ServiceDisplayName,
    string? ProblemClassificationId,
    string? ProblemClassificationDisplayName,
    string? TechnicalResourceId,
    string? SupportPlanType,
    string? SupportEngineerEmailAddress,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? ModifiedOn,
    List<SupportTicketCommunication>? Communications = null);
