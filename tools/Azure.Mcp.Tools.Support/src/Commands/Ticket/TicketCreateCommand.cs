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
        Create a new Azure support ticket (support request). Tickets are worked by human Microsoft support
        engineers, so only create one when the user has explicitly asked to escalate to support. Before calling:
        (1) run available diagnostics (resource health, AppLens, Azure Monitor) and include the evidence (errors,
        correlation IDs, timestamps) in the description; (2) check for duplicates with the ticket list tool;
        (3) get a problem classification ID from the service and problem classification list tools; (4) show the
        user the exact title, severity and description that will be sent (call out 'critical' or
        'highestcriticalimpact' severity prominently) plus contact details, and wait for their explicit approval.
        Never include secrets (keys, passwords, tokens, connection strings); requests containing them are
        rejected. Advanced diagnostic consent is not set by this tool; Microsoft support requests it through
        its own flow. Required: title, description, problem classification, severity, and contact details.
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

        var secretCategories = SupportTicketSecretScanner.Scan(options.Title, options.Description, options.TechnicalResource, options.TicketName);
        if (secretCategories.Count > 0)
        {
            validationResult.AddError(
                $"The ticket text appears to contain secrets ({string.Join("; ", secretCategories)}). Remove them from the ticket text before creating the ticket; support tickets must not contain credentials.",
                "Ticket text contains secrets.");
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
