// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Support.Models;

public sealed record SupportTicketCreateRequest(
    string Title,
    string Description,
    string ProblemClassificationId,
    string Severity,
    string FirstName,
    string LastName,
    string Email,
    string ContactMethod,
    string? PhoneNumber,
    string Timezone,
    string Country,
    string Language,
    string? TechnicalResourceId,
    string? TicketName);
