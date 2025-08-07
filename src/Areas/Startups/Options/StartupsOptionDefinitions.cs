// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Areas.Startups.Options;

public static class StartupsOptionDefinitions
{
    public const string ResourceGroupParam = "resource-group";
    public const string StorageAccountParam = "storage-account";
    public const string SourcePathParam = "source-path";

    public const string OverwriteParam = "overwrite";

    public const string DeployTypeParam = "deploy-type";
    public const string ReactProjectParam = "react-project";
    public const string BuildParam = "build";
    public const string BuildPathParam = "build-path";

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
        "The path to the source directory containing website files"
    )
    {
        IsRequired = false
    };

    public static readonly Option<bool> Overwrite = new(
        $"--{OverwriteParam}",
        "Overwrite existing files when deploying"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DeployType = new(
        $"--{DeployTypeParam}",
        "type of deployment: 'static' for HTML/CSS/JS files, 'react' for React apps"
    )
    {
        IsRequired = true
    };
    public static readonly Option<string> ReactProject = new(
        $"--{ReactProjectParam}",
        "The path to the React project directory (contains package.json)"
    )
    {
        IsRequired = false
    };
    public static readonly Option<bool> Build = new(
        $"--{BuildParam}",
        "whether to build the React project before deployment"
    )
    {
        IsRequired = false
    };
    public static readonly Option<string> BuildPath = new(
        $"--{BuildPathParam}",
        "custom build output path (defaults to 'build' folder)"
    )
    {
        IsRequired = false
    };
}
