// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.ActivityLog;

namespace Azure.Mcp.Tools.Monitor.Options.ActivityLog;

public static class ActivityLogOptionDefinitions
{
    public const string ResourceNameName = "resource-name";
    public const string ResourceTypeName = "resource-type";
    public const string HoursName = "hours";
    public const string EventLevelName = "event-level";
    public const string TopName = "top";

    public static readonly Option<string> ResourceName = new(
        $"--{ResourceNameName}"
    )
    {
        Description = "The name of the Azure resource to retrieve activity logs for.",
        Required = true
    };

    public static readonly Option<string> ResourceType = new(
        $"--{ResourceTypeName}"
    )
    {
        Description = "The type of the Azure resource (e.g., 'Microsoft.Storage/storageAccounts'). Only provide this if needed to disambiguate between multiple resources with the same name.",
        Required = false
    };

    public static readonly Option<double> Hours = new(
        $"--{HoursName}"
    )
    {
        Description = "The number of hours prior to now to retrieve activity logs for.",
        DefaultValueFactory = _ => 24.0,
        Required = false
    };

    public static readonly Option<ActivityLogEventLevel?> EventLevel = new(
        $"--{EventLevelName}"
    )
    {
        Description = "The level of activity logs to retrieve. Valid levels are: Critical, Error, Informational, Verbose, Warning. If not provided, returns all levels.",
        Required = false
    };

    public static readonly Option<int> Top = new(
        $"--{TopName}"
    )
    {
        Description = "The maximum number of activity logs to retrieve.",
        DefaultValueFactory = _ => 10,
        Required = false
    };
}
