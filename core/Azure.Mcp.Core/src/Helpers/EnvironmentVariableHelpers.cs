// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Helpers
{
    public static class EnvironmentHelpers
    {
        public static bool GetEnvironmentVariableAsBool(string envVarName)
        {
            return Environment.GetEnvironmentVariable(envVarName) switch
            {
                "true" => true,
                "True" => true,
                "T" => true,
                "1" => true,
                _ => false
            };
        }
    }
}
