// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Extensions.AI.Evaluation;

namespace Azure.Mcp.Tools.Foundry.Models;

public class AgentsQueryAndEvaluateResult : AgentsConnectResult
{
    [JsonPropertyName("evaluators")]
    public List<string>? Evaluators { get; set; }

    [JsonPropertyName("evaluationResult")]
    public EvaluationResult? EvaluationResult { get; set; }
}
