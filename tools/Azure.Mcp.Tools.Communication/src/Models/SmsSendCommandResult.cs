// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Communication.Models;

public class SmsSendCommandResult
{
    [JsonPropertyName("results")]
    public List<SmsResult> Results { get; }

    public SmsSendCommandResult(List<SmsResult> results)
    {
        Results = results;
    }
}
