// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Options;

public static class ExtensionOptionDefinitions
{
    public static class Az
    {
        public const string CommandName = "command";

        public static readonly Option<string> Command = new(
            $"--{CommandName}",
            "The Azure CLI command to execute (without the 'az' prefix). For example: 'group list'."
        )
        {
            IsRequired = true
        };
    }

    public static class Azd
    {
        public const string CommandName = "command";

        public static readonly Option<string> Command = new(
            $"--{CommandName}",
            """
                The Azure Developer CLI command and arguments to execute (without the 'azd' prefix).
                Examples:
                - up
                - env list
                - env get-values
                """
        )
        {
            IsRequired = false
        };


        public const string CwdName = "cwd";

        public static readonly Option<string> Cwd = new(
            $"--{CwdName}",
            "The current working directory for the command. This is the directory where the command will be executed."
        )
        {
            IsRequired = true
        };

        public const string EnvironmentName = "environment";
        public static readonly Option<string> Environment = new(
            $"--{EnvironmentName}",
            """
                The name of the azd environment to use. This is typically the name of the Azure environment (e.g., 'prod', 'dev', 'test', 'staging').
                Always set environments for azd commands that support -e, --environment argument.
                """
        )
        {
            IsRequired = false
        };

        public const string LearnName = "learn";
        public static readonly Option<bool> Learn = new(
            $"--{LearnName}",
            () => false,
            """
                Flag to indicate whether to learn best practices and usage patterns for azd tool.
                Always run this command with learn=true and empty command on first run.
                """
        )
        {
            IsRequired = false
        };
    }
}
