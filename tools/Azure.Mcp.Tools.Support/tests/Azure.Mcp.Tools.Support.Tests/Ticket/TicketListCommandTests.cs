// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Support.Commands;
using Azure.Mcp.Tools.Support.Commands.Ticket;
using Azure.Mcp.Tools.Support.Models;
using Azure.Mcp.Tools.Support.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Support.Tests.Ticket;

public class TicketListCommandTests : SubscriptionCommandUnitTestsBase<TicketListCommand, ISupportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTickets_WithFilterAndTop()
    {
        var tickets = new List<SupportTicket>
        {
            new("ticket1", "/subscriptions/sub123/providers/Microsoft.Support/supportTickets/ticket1", "2401010010000001",
                "VM not starting", "desc", "Open", "Moderate", null, null, null, null, null, null, null, null, null)
        };
        Service.ListTicketsAsync("sub123", "status eq 'Open'", 10, null, Arg.Any<CancellationToken>()).Returns(tickets);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--filter", "status eq 'Open'", "--top", "10");

        var result = ValidateAndDeserializeResponse(response, SupportJsonContext.Default.TicketListCommandResult);
        var ticket = Assert.Single(result.Tickets);
        Assert.Equal("ticket1", ticket.Name);
        Assert.Equal("Open", ticket.Status);
        await Service.Received(1).ListTicketsAsync("sub123", "status eq 'Open'", 10, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub123", HttpStatusCode.OK)]
    [InlineData("--subscription sub123 --top 0", HttpStatusCode.BadRequest)]
    [InlineData("--subscription sub123 --top 101", HttpStatusCode.BadRequest)]
    public async Task ExecuteAsync_ValidatesInput(string args, HttpStatusCode expectedStatus)
    {
        Service.ListTicketsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(expectedStatus, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListTicketsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
