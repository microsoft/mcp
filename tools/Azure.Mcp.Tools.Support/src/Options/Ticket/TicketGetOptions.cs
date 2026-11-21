// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Support.Options.Ticket;

public class TicketGetOptions : ISubscriptionOption
{
    [Option(Description = SupportOptionDescriptions.Ticket)]
    public required string Ticket { get; set; }

    [Option(Description = SupportOptionDescriptions.IncludeCommunications)]
    public bool IncludeCommunications { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
