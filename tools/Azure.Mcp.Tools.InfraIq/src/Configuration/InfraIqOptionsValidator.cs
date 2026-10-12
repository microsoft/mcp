// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;

namespace Azure.Mcp.Tools.InfraIq.Configuration;

/// <summary>
/// Validates the ingress origin at startup. An absent or blank origin selects automatic routing and is accepted; any
/// non-blank unapproved origin fails closed.
/// </summary>
public sealed class InfraIqOptionsValidator : IValidateOptions<InfraIqOptions>
{
    public ValidateOptionsResult Validate(string? name, InfraIqOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ArmIngressOrigin))
        {
            return ValidateOptionsResult.Success;
        }

        return InfraIqArmIngress.TryParseOrigin(options.ArmIngressOrigin, out _, out var error)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(error);
    }
}
