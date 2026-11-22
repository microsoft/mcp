// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Support.Options.Ticket;

public class TicketCreateOptions : ISubscriptionOption
{
    [Option(Description = SupportOptionDescriptions.Title)]
    public required string Title { get; set; }

    [Option(Description = SupportOptionDescriptions.Description)]
    public required string Description { get; set; }

    [Option(Description = SupportOptionDescriptions.ProblemClassification)]
    public required string ProblemClassification { get; set; }

    [Option(Description = SupportOptionDescriptions.Severity)]
    public required string Severity { get; set; }

    [Option(Description = SupportOptionDescriptions.FirstName)]
    public required string FirstName { get; set; }

    [Option(Description = SupportOptionDescriptions.LastName)]
    public required string LastName { get; set; }

    [Option(Description = SupportOptionDescriptions.Email)]
    public required string Email { get; set; }

    [Option(Description = SupportOptionDescriptions.Timezone)]
    public required string Timezone { get; set; }

    [Option(Description = SupportOptionDescriptions.Country)]
    public required string Country { get; set; }

    [Option(Description = SupportOptionDescriptions.Language)]
    public required string Language { get; set; }

    [Option(Description = SupportOptionDescriptions.ContactMethod)]
    public string? ContactMethod { get; set; }

    [Option(Description = SupportOptionDescriptions.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    [Option(Description = SupportOptionDescriptions.TechnicalResource)]
    public string? TechnicalResource { get; set; }

    [Option(Description = SupportOptionDescriptions.TicketName)]
    public string? TicketName { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
