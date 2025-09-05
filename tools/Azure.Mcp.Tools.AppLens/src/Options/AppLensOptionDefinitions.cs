// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppLens.Options;

public static class AppLensOptionDefinitions
{
    public static class Resource
    {
        public const string QuestionName = "question";
        public const string ResourceNameName = "resource-name";
        public const string SubscriptionName = "subscription";
        public const string ResourceGroupName = "resource-group";
        public const string ResourceTypeName = "resource-type";

        public static readonly Option<string> Question = new(
            $"--{QuestionName}")
        {
            Description = "User question",
            Required = true
        };

        public static readonly Option<string> ResourceName = new(
            $"--{ResourceNameName}")
        {
            Description = "The name of the resource to investigate or diagnose",
            Required = true
        };

        public static readonly Option<string?> Subscription = new(
            $"--{SubscriptionName}")
        {
            Description = "The subscription the resource belongs to. Try to get this information using the Azure CLI tool before asking the user.",
            Required = true
        };
        public static readonly Option<string?> ResourceGroup = new(
            $"--{ResourceGroupName}")
        {
            Description = "The name of the Azure resource group. This is a logical container for Azure resources. Try to get this information using the Azure CLI tool before asking the user.",
            Required = true
        };
        public static readonly Option<string?> ResourceType = new(
            $"--{ResourceTypeName}")
        {
            Description = "Resource type. Try to get this information using the Azure CLI tool before asking the user.",
            Required = true
        };
    }
}
