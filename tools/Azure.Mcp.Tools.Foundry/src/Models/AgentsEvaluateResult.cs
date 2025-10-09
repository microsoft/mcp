// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Extensions.AI.Evaluation;

namespace Azure.Mcp.Tools.Foundry.Models;

public class AgentsEvaluateResult
{
    [JsonPropertyName("evaluator")]
    public string? Evaluator { get; set; }

    [JsonPropertyName("evaluationResult")]
    public EvaluationResult? EvaluationResult { get; set; }
}
