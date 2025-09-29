// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Communication.Options;

public static class CommunicationOptionDefinitions
{
    public const string ConnectionStringName = "connection-string";
    public const string FromName = "from";
    public const string ToName = "to";
    public const string MessageName = "message";
    public const string EnableDeliveryReportName = "enable-delivery-report";
    public const string TagName = "tag";

    public static readonly Option<string> ConnectionString =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<string>(
            ConnectionStringName,
            "The connection string for the Azure Communication Services resource. You can find this in the Azure portal under your Communication Services resource.",
            required: true
        ).AsRequired();

    public static readonly Option<string> From =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<string>(
            FromName,
            "The SMS-enabled phone number associated with your Communication Services resource (in E.164 format, e.g., +14255550123). Can also be a short code or alphanumeric sender ID.",
            required: true
        ).AsRequired();

    public static readonly Option<string[]> To =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<string[]>(
            ToName,
            "The recipient phone number(s) in E.164 international standard format (e.g., +14255550123). Multiple numbers can be provided.",
            required: true
        ).AsRequired();

    public static readonly Option<string> Message =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<string>(
            MessageName,
            "The SMS message content to send to the recipient(s).",
            required: true
        ).AsRequired();

    public static readonly Option<bool> EnableDeliveryReport =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<bool>(
            EnableDeliveryReportName,
            "Whether to enable delivery reporting for the SMS message. When enabled, events are emitted when delivery is successful.",
            defaultValue: false
        ).AsOptional();

    public static readonly Option<string> Tag =
        new Azure.Mcp.Core.Models.Option.OptionDefinition<string>(
            TagName,
            "Optional custom tag to apply to the SMS message for tracking purposes."
        ).AsOptional();
}