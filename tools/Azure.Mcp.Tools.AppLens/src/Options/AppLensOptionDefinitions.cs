// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppLens.Options;

public static class AppLensOptionDefinitions
{
    public const string QuestionName = "question";
    public const string ResourceName = "resource";
    public const string ResourceTypeName = "resource-type";
    public const string ResourceGroupName = "resource-group";
    public const string SubscriptionName = "subscription";

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

    public static readonly Option<string?> Subscription = new(
        $"--{SubscriptionName}")
    {
        Description = "Azure subscription ID or name. Provide this when disambiguating between multiple resources of the same name.",
        Required = false
    };

    public static readonly Option<string?> ResourceGroup = new(
        $"--{ResourceGroupName}")
    {
        Description = "Azure resource group name. Provide this when disambiguating between multiple resources of the same name.",
        Required = false
    };

    public static readonly Option<string?> ResourceType = new(
        $"--{ResourceTypeName}")
    {
        Description = "Resource type. Provide this when disambiguating between multiple resources of the same name.",
        Required = false
    };
}
