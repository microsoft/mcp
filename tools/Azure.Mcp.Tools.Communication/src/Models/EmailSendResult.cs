// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Communication.Models;

/// <summary>
/// Represents the result of an email send operation.
/// </summary>
public class EmailSendResult
{
    /// <summary>
    /// The ID of the email message.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// The status of the email send operation.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
