// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Communication.Models;

namespace Azure.Mcp.Tools.Communication.Services;

public interface ICommunicationService
{
    Task<List<SmsResult>> SendSmsAsync(
        string connectionString,
        string from,
        string[] to,
        string message,
        bool enableDeliveryReport = false,
        string? tag = null,
        RetryPolicyOptions? retryPolicy = null);
}
