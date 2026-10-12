// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;

namespace Azure.Mcp.Tools.AIInfraOptimization.Configuration;

/// <summary>
/// Validates the ingress origin at startup. An absent or blank origin selects automatic routing and is accepted; any
/// non-blank unapproved origin fails closed.
/// </summary>
public sealed class AIInfraOptimizationOptionsValidator : IValidateOptions<AIInfraOptimizationOptions>
{
    public ValidateOptionsResult Validate(string? name, AIInfraOptimizationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ArmIngressOrigin))
        {
            return ValidateOptionsResult.Success;
        }

        return AIInfraOptimizationArmIngress.TryParseOrigin(options.ArmIngressOrigin, out _, out var error)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(error);
    }
}
