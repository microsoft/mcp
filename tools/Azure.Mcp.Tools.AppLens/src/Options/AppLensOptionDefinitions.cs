// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppLens.Options;

public static class AppLensOptionDefinitions
{
    public const string QuestionName = "question";
    public const string ResourceName = "resource";
    public const string ResourceTypeName = "resource-type";

    public static readonly Option<string> Question = new(
        $"--{QuestionName}")
    {
        Description = "User question",
        Required = true
    };

    public static readonly Option<string> Resource = new(
        $"--{ResourceName}")
    {
        Description = "The name of the resource to investigate or diagnose",
        Required = true
    };

    public static readonly Option<string?> ResourceType = new(
        $"--{ResourceTypeName}")
    {
        Description = "Resource type. Try to get this information using the Azure CLI tool before asking the user.",
        Required = true
    };
}
