// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Server.Tests.Infrastructure;

internal static class ServerTestOptions
{
    private const string ThreeStepToolDiscoveryEnvironmentVariable = "AZURE_MCP_TEST_THREE_STEP_TOOL_DISCOVERY";

    public static bool IsThreeStepToolDiscoveryEnabled()
    {
        var configured = Environment.GetEnvironmentVariable(ThreeStepToolDiscoveryEnvironmentVariable);
        return !string.IsNullOrWhiteSpace(configured) &&
            (configured.Equals("true", StringComparison.OrdinalIgnoreCase) || configured == "1");
    }

    public static string GetServerStartArguments(string arguments)
        => IsThreeStepToolDiscoveryEnabled()
            ? $"{arguments} --three-step-tool-discovery"
            : arguments;
}
