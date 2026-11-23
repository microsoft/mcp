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

public class TicketGetCommandTests : SubscriptionCommandUnitTestsBase<TicketGetCommand, ISupportService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTicketWithCommunications()
    {
        var ticket = new SupportTicket("ticket1", null, "2401010010000001", "VM not starting", "desc", "Open", "Moderate",
            null, null, null, null, null, null, null, null, null,
            [new("comm1", "Update", "We are looking into it", "support@microsoft.com", "Web", "Outbound", null)]);
        Service.GetTicketAsync("sub123", "ticket1", true, null, Arg.Any<CancellationToken>()).Returns(ticket);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--ticket", "ticket1", "--include-communications", "true");

        var result = ValidateAndDeserializeResponse(response, SupportJsonContext.Default.TicketGetCommandResult);
        Assert.Equal("ticket1", result.Ticket.Name);
        var communication = Assert.Single(result.Ticket.Communications!);
        Assert.Equal("Update", communication.Subject);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotRequestCommunicationsByDefault()
    {
        Service.GetTicketAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new SupportTicket("ticket1", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--ticket", "ticket1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetTicketAsync("sub123", "ticket1", false, null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub123", HttpStatusCode.BadRequest)]
    [InlineData("--ticket ticket1", HttpStatusCode.BadRequest)]
    public async Task ExecuteAsync_ValidatesInput(string args, HttpStatusCode expectedStatus)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(expectedStatus, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetTicketAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--ticket", "ticket1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
