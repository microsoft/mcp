// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubUsageSnapshot(
    string HubName,
    DateTimeOffset SnapshotTime,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    IoTHubDeviceCountStats ConnectedDeviceCount,
    IoTHubDeviceCountStats TotalDeviceCount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? DailyMessageQuotaUsed,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<IoTHubDailyMessageUsage>? DailyMessageQuotaUsedByDay,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? TotalMessagesUsed,
    double? D2CMessageCount,
    double? ThrottlingErrors,
    double? PeakHourlyThrottlingErrors,
    string Sku,
    long Units,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? RecommendedSku);
