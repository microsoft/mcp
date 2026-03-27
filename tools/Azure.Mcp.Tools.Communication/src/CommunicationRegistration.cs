// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Communication.Commands.Email;
using Azure.Mcp.Tools.Communication.Commands.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Communication;

public sealed class CommunicationRegistration : IAreaRegistration
{
    public static string AreaName => "communication";

    public static string AreaTitle => "Azure Communication Services";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Communication services operations - Commands for managing Azure Communication Services - supports sending SMS",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "email",
                Description = "Email messaging operations - sending email messages to one or more recipients using Azure Communication Services.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "60f79b69-9e90-4f07-9bf4-bd4452f1143d",
                        Name = "send",
                        Description = "Send emails to one or multiple recipients to the given email-address. The emails can be plain text or HTML formatted. You can include a subject, custom sender name, CC and BCC recipients, and reply-to addresses.",
                        Title = "Send",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = true,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "endpoint",
                                Description = "The Communication Services URI endpoint (e.g., https://myservice.communication.azure.com). Required for credential authentication.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "from",
                                Description = "The email address to send from (must be from a verified domain)",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sender-name",
                                Description = "The display name of the sender",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "to",
                                Description = "The recipient email address(es) to send the email to.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "cc",
                                Description = "CC recipient email addresses",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "bcc",
                                Description = "BCC recipient email addresses",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "subject",
                                Description = "The email subject",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "message",
                                Description = "The email message content to send to the recipient(s).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "is-html",
                                Description = "Flag indicating whether the message content is HTML",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "reply-to",
                                Description = "Reply-to email addresses",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(SmsSendCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "sms",
                Description = "SMS messaging operations - sending SMS messages to one or more recipients using Azure Communication Services.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a0dc94f3-25ac-4971-a552-0d90fd57e902",
                        Name = "send",
                        Description = "Sends SMS messages to one or more recipients to the given phone-number. You can enable delivery reports and receipt tracking, broadcast SMS, and tag messages for easier tracking. Returns message IDs and delivery status for each sent message.",
                        Title = "Send",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = true,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "endpoint",
                                Description = "The Communication Services URI endpoint (e.g., https://myservice.communication.azure.com). Required for credential authentication.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "from",
                                Description = "The SMS-enabled phone number associated with your Communication Services resource (in E.164 format, e.g., +14255550123). Can also be a short code or alphanumeric sender ID.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "to",
                                Description = "The recipient phone number(s) in E.164 international standard format (e.g., +14255550123). Multiple numbers can be provided.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "message",
                                Description = "The SMS message content to send to the recipient(s).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "enable-delivery-report",
                                Description = "Whether to enable delivery reporting for the SMS message. When enabled, events are emitted when delivery is successful.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tag",
                                Description = "Optional custom tag to apply to the SMS message for tracking purposes.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(SmsSendCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommunicationService, CommunicationService>();
        services.AddSingleton<SmsSendCommand>();
        services.AddSingleton<EmailSendCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(SmsSendCommand) => serviceProvider.GetRequiredService<SmsSendCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in communication area.")
        };
}
