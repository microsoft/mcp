// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.Communication.Options;

public static class CommunicationOptionDefinitions
{
    public const string ConnectionStringName = "connection-string";
    public const string FromName = "from";
    public const string ToName = "to";
    public const string MessageName = "message";
    public const string EnableDeliveryReportName = "enable-delivery-report";
    public const string TagName = "tag";

    public static readonly Option<string> ConnectionString = new(
        $"--{ConnectionStringName}",
        "The connection string for the Azure Communication Services resource. You can find this in the Azure portal under your Communication Services resource."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> From = new(
        $"--{FromName}",
        "The SMS-enabled phone number associated with your Communication Services resource (in E.164 format, e.g., +14255550123). Can also be a short code or alphanumeric sender ID."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string[]> To = new(
        $"--{ToName}",
        "The recipient phone number(s) in E.164 international standard format (e.g., +14255550123). Multiple numbers can be provided."
    )
    {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string> Message = new(
        $"--{MessageName}",
        "The SMS message content to send to the recipient(s)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<bool> EnableDeliveryReport = new(
        $"--{EnableDeliveryReportName}",
        () => false,
        "Whether to enable delivery reporting for the SMS message. When enabled, events are emitted when delivery is successful."
    );

    public static readonly Option<string> Tag = new(
        $"--{TagName}",
        "Optional custom tag to apply to the SMS message for tracking purposes."
    );
}