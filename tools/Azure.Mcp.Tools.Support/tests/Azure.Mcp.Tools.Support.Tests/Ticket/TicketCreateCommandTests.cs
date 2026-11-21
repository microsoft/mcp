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

public class TicketCreateCommandTests : SubscriptionCommandUnitTestsBase<TicketCreateCommand, ISupportService>
{
    private const string ProblemClassificationId =
        "/providers/Microsoft.Support/services/06bfd9d3-516b-d5c6-5802-169c800dec89/problemClassifications/e12e3d1d-7fa0-af33-c6d0-3c50df9658a3";

    private const string RequiredArgs =
        "--subscription sub123 --title \"VM not starting\" --description \"Correlation ID abc\" " +
        "--problem-classification " + ProblemClassificationId + " --severity moderate " +
        "--first-name Ada --last-name Lovelace --email ada@contoso.com " +
        "--timezone \"Pacific Standard Time\" --country USA --language en-US";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Fact]
    public void Metadata_IsDestructiveAndNotReadOnly()
    {
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_MapsOptionsToServiceRequest()
    {
        Service.CreateTicketAsync(Arg.Any<string>(), Arg.Any<SupportTicketCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new SupportTicket("ticket1", null, "2401010010000001", "VM not starting", "Correlation ID abc", "Open", "Moderate",
                null, null, ProblemClassificationId, null, null, null, null, null, null));

        var response = await ExecuteCommandAsync(RequiredArgs + " --technical-resource /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1");

        var result = ValidateAndDeserializeResponse(response, SupportJsonContext.Default.TicketCreateCommandResult);
        Assert.Equal("2401010010000001", result.Ticket.SupportTicketId);
        await Service.Received(1).CreateTicketAsync(
            "sub123",
            Arg.Is<SupportTicketCreateRequest>(r =>
                r.Title == "VM not starting" &&
                r.Description == "Correlation ID abc" &&
                r.ProblemClassificationId == ProblemClassificationId &&
                r.Severity == "moderate" &&
                r.FirstName == "Ada" &&
                r.LastName == "Lovelace" &&
                r.Email == "ada@contoso.com" &&
                r.ContactMethod == "email" &&
                r.Timezone == "Pacific Standard Time" &&
                r.Country == "USA" &&
                r.Language == "en-US" &&
                !r.AdvancedDiagnosticConsent &&
                r.TechnicalResourceId == "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1" &&
                r.TicketName == null),
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", HttpStatusCode.OK)]
    [InlineData(" --severity urgent", HttpStatusCode.BadRequest)]
    [InlineData(" --contact-method fax", HttpStatusCode.BadRequest)]
    [InlineData(" --contact-method phone", HttpStatusCode.BadRequest)]
    [InlineData(" --contact-method phone --phone-number 5550100", HttpStatusCode.OK)]
    [InlineData(" --email not-an-email", HttpStatusCode.BadRequest)]
    [InlineData(" --problem-classification /subscriptions/sub123", HttpStatusCode.BadRequest)]
    public async Task ExecuteAsync_ValidatesInput(string extraArgs, HttpStatusCode expectedStatus)
    {
        Service.CreateTicketAsync(Arg.Any<string>(), Arg.Any<SupportTicketCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new SupportTicket("ticket1", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null));

        // Later occurrences of an option override earlier ones, so extraArgs replaces the valid defaults.
        var response = await ExecuteCommandAsync(RequiredArgs + extraArgs);

        Assert.Equal(expectedStatus, response.Status);
    }

    [Theory]
    [InlineData("--title")]
    [InlineData("--description")]
    [InlineData("--problem-classification")]
    [InlineData("--severity")]
    [InlineData("--email")]
    [InlineData("--country")]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenRequiredOptionMissing(string missingOption)
    {
        var args = new List<string>
        {
            "--subscription", "sub123",
            "--title", "t",
            "--description", "d",
            "--problem-classification", ProblemClassificationId,
            "--severity", "minimal",
            "--first-name", "Ada",
            "--last-name", "Lovelace",
            "--email", "ada@contoso.com",
            "--timezone", "UTC",
            "--country", "USA",
            "--language", "en-US"
        };
        var index = args.IndexOf(missingOption);
        args.RemoveRange(index, 2);

        var response = await ExecuteCommandAsync([.. args]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().CreateTicketAsync(Arg.Any<string>(), Arg.Any<SupportTicketCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.CreateTicketAsync(Arg.Any<string>(), Arg.Any<SupportTicketCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(RequiredArgs);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
