// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Support.Options.Ticket;

public class TicketListOptions : ISubscriptionOption
{
    [Option(Description = SupportOptionDescriptions.Filter)]
    public string? Filter { get; set; }

    [Option(Description = SupportOptionDescriptions.Top)]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
