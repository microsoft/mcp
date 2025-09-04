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
        public const string SubscriptionNameOrIdName = "subscription-name-or-id";
        public const string ResourceGroupName = "resource-group";
        public const string ResourceTypeName = "resource-type";

        public static readonly Option<string> Question = new(
            $"--{QuestionName}",
            "User question")
        {
            IsRequired = true
        };

        public static readonly Option<string> ResourceName = new(
            $"--{ResourceNameName}",
            "The name of the resource to investigate or diagnose")
        {
            IsRequired = true
        };

        public static readonly Option<string?> SubscriptionNameOrId = new(
            $"--{SubscriptionNameOrIdName}",
            "Optional subscription name or ID. Only provide if needed to disambiguate between multiple resources with the same name");

        public static readonly Option<string?> ResourceGroup = new(
            $"--{ResourceGroupName}",
            "The name of the Azure resource group. This is a logical container for Azure resources.");

        public static readonly Option<string?> ResourceType = new(
            $"--{ResourceTypeName}",
            "Optional resource type. Only provide if needed to disambiguate between multiple resources with the same name");
    }
}
