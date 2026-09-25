// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Communication.Models;

public sealed record SmsResult(
    string? MessageId,
    string? To,
    bool Successful,
    int HttpStatusCode,
    string? ErrorMessage);
