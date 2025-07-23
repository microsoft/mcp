// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Areas.Startups.Options;

public static class StartupsOptionDefinitions
{

    public const string SubscriptionIdParam = "subscription";

    public static readonly Option<string> SubscriptionId = new(
        $"--{SubscriptionIdParam}",
        "The Id of the subscription"
    )
    {
        IsRequired = true
    };
}
