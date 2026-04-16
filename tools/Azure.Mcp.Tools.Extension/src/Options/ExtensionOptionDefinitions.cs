// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Options;

public static class ExtensionOptionDefinitions
{
    public static class CliGenerate
    {
        public const string IntentName = "intent";
        public static readonly Option<string> Intent = new(
            $"--{IntentName}"
        )
        {
            Description = "The user intent of the task to be solved by using the CLI tool. This user intent will be used to generate the appropriate CLI command to accomplish the desirable goal.",
            Required = true
        };

        public const string CliTypeName = "cli-type";
        public static readonly Option<string> CliType = new(
            $"--{CliTypeName}"
        )
        {
            Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI.",
            Required = true
        };
    }

    public static class CliInstall
    {
        public const string CliTypeName = "cli-type";
        public static readonly Option<string> CliType = new(
            $"--{CliTypeName}"
        )
        {
            Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI, 'azd' for Azure Developer CLI, and 'func' for Azure Functions Core Tools CLI.",
            Required = true
        };
    }
}
