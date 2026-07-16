// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// The messages-used-against-quota value for a single UTC day.
/// </summary>
/// <param name="Date">The UTC calendar date in yyyy-MM-dd format.</param>
/// <param name="MessagesUsed">The daily message quota consumed on that date, or null when no data was reported.</param>
public record IoTHubDailyMessageUsage(
    string Date,
    double? MessagesUsed);
