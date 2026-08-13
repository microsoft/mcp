// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace McpToolEvaluator.Core.Models;

public enum PromptInteraction
{
    [EnumMember(Value = PromptInteractionExtensions.None)]
    None,

    [EnumMember(Value = PromptInteractionExtensions.ClarificationRequired)]
    ClarificationRequired,

    [EnumMember(Value = PromptInteractionExtensions.ContextRequired)]
    ContextRequired
}
