// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Models;

internal static class CliInstallTypeExtensions
{
    internal static string ToValue(this CliInstallType cliType) => cliType switch
    {
        CliInstallType.Az => Constants.AzureCliType,
        CliInstallType.Azd => Constants.AzureDeveloperCliType,
        CliInstallType.Func => Constants.AzureFunctionsCoreToolsCliType,
        _ => throw new ArgumentOutOfRangeException(nameof(cliType), cliType, null)
    };
}
