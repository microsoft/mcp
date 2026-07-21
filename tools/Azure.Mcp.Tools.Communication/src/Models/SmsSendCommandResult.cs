// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Communication.Models;

public sealed record SmsSendCommandResult(List<SmsResult> Results);
