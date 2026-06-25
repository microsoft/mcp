// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppConfig.Options;

/// <summary>
/// Common option descriptions for App Configuration.
/// </summary>
internal static class AppConfigOptionDescriptions
{
    internal const string Account = "The name of the App Configuration store (e.g., my-appconfig).";
    internal const string Key = "The name of the key to access within the App Configuration store.";
    internal const string Label = "The label to apply to the configuration key. Labels are used to group and organize settings.";
}
