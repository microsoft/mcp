// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;

namespace Azure.Mcp.Tools.InfraIq.Configuration;

/// <summary>
/// Validates the ingress origin at startup. Setup defaults an absent or blank origin to the approved development
/// origin, so a blank value here is tolerated; any non-blank unapproved origin fails closed.
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
