// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Startups.Options;

public static class StartupsOptionDefinitions
{
    public const string ResourceGroupParam = "resource-group";
    public const string StorageAccountParam = "storage-account";
    public const string SourcePathParam = "source-path";

    public const string OverwriteParam = "overwrite";

    public static readonly Option<string> ResourceGroup = new(
        $"--{ResourceGroupParam}",
        "The name of the Azure resource group"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> StorageAccount = new(
        $"--{StorageAccountParam}",
        "The name of the storage account"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> SourcePath = new(
        $"--{SourcePathParam}",
        "Path to your web application's root directory. The deployment process will automatically " +
        "detect your project type and use the appropriate build output directory. " +
        "For simple static websites, this should be the directory containing your web content. " +
        "For web applications using modern frameworks, the build output will be automatically located and deployed."
    )
    {
        IsRequired = true,
    };

    public static readonly Option<bool> Overwrite = new(
        $"--{OverwriteParam}",
        "Overwrite existing files when deploying"
    )
    {
        IsRequired = true
    };
}
