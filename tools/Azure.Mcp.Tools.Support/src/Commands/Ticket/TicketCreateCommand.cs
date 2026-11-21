// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Mail;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Support.Models;
using Azure.Mcp.Tools.Support.Options.Ticket;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Support.Commands.Ticket;

[CommandMetadata(
    Id = "1343cf4e-2546-4995-9287-377fa3c11888",
    Name = "create",
    Title = "Create Azure Support Ticket",
    Description = """
        Create a new Azure support ticket (support request) in a subscription. This opens a case with Microsoft
        support and may notify support engineers, so only call it as a last resort. Before calling: (1) run available
        diagnostics first, such as resource health, AppLens, Azure Monitor, or storage disk diagnose tools, because
        they often resolve the issue without a ticket; (2) check existing tickets with the ticket list tool to avoid
        duplicates; (3) find a valid problem classification ID with the service list and problem classification list
        tools; (4) include the diagnostic evidence (error messages, correlation/activity IDs, timestamps, resource
        health status, AppLens findings) in the description; and (5) show the user the full ticket details, including
        contact information and severity, and get their explicit confirmation. Required: title, description, problem
        classification, severity, and contact details (first name, last name, email, time zone, country, language).
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed partial class TicketCreateCommand(ILogger<TicketCreateCommand> logger, ISupportService supportService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TicketCreateOptions, TicketCreateCommand.TicketCreateCommandResult>(subscriptionResolver)
{
    private static readonly string[] s_severities = ["minimal", "moderate", "critical", "highestcriticalimpact"];
    private static readonly string[] s_contactMethods = ["email", "phone"];

    private readonly ILogger<TicketCreateCommand> _logger = logger;
    private readonly ISupportService _supportService = supportService;

    public override JsonTypeInfo<TicketCreateCommandResult>? ResultTypeInfo => SupportJsonContext.Default.TicketCreateCommandResult;

    public override void ValidateOptions(TicketCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.Severity) && !s_severities.Contains(options.Severity, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"Invalid --severity '{options.Severity}'. Allowed values: {string.Join(", ", s_severities)}.",
                "Invalid severity.");
        }

        var contactMethod = options.ContactMethod ?? "email";
        if (!s_contactMethods.Contains(contactMethod, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"Invalid --contact-method '{options.ContactMethod}'. Allowed values: {string.Join(", ", s_contactMethods)}.",
                "Invalid contact method.");
        }
        else if (contactMethod.Equals("phone", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.PhoneNumber))
        {
            validationResult.AddError(
                "--phone-number is required when --contact-method is 'phone'.",
                "Missing phone number for phone contact method.");
        }

        if (!string.IsNullOrWhiteSpace(options.Email) && !MailAddress.TryCreate(options.Email, out _))
        {
            validationResult.AddError("--email must be a valid email address.", "Invalid email address.");
        }

        if (!string.IsNullOrWhiteSpace(options.ProblemClassification) && !ProblemClassificationIdRegex().IsMatch(options.ProblemClassification))
        {
            validationResult.AddError(
                "--problem-classification must be in the format '/providers/Microsoft.Support/services/{serviceName}/problemClassifications/{problemClassificationName}'.",
                "Invalid problem classification ID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TicketCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var request = new SupportTicketCreateRequest(
                options.Title,
                options.Description,
                options.ProblemClassification,
                options.Severity,
                options.FirstName,
                options.LastName,
                options.Email,
                options.ContactMethod ?? "email",
                options.PhoneNumber,
                options.Timezone,
                options.Country,
                options.Language,
                options.AdvancedDiagnosticConsent,
                options.TechnicalResource,
                options.TicketName);

            var ticket = await _supportService.CreateTicketAsync(
                options.Subscription!,
                request,
                options.Tenant,
                cancellationToken);

            SetResult(context, new(ticket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    [GeneratedRegex(@"^/providers/Microsoft\.Support/services/[^/]+/problemClassifications/[^/]+$", RegexOptions.IgnoreCase)]
    private static partial Regex ProblemClassificationIdRegex();

    public sealed record TicketCreateCommandResult(SupportTicket Ticket);
}
