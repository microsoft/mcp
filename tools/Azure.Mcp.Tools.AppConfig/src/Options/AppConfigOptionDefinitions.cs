// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppConfig.Options;

public static class AppConfigOptionDefinitions
{
    public const string AccountName = "account";
    public const string KeyName = "key";
    public const string ValueName = "value";
    public const string LabelName = "label";
    public const string ContentTypeName = "content-type";
    public const string TagsName = "tags";

    public static readonly Option<string> Account = new(
        $"--{AccountName}"
    )
    {
        Description = "The name of the App Configuration store (e.g., my-appconfig).",
        Required = true
    };

    public static readonly Option<string> Key = new(
        $"--{KeyName}"
    )
    {
        Description = "The name of the key to access within the App Configuration store.",
        Required = true
    };

    public static readonly Option<string> Value = new(
        $"--{ValueName}"
    )
    {
        Description = "The value to set for the configuration key.",
        Required = true
    };

    public static readonly Option<string> Label = new(
        $"--{LabelName}"
    )
    {
        Description = "The label to apply to the configuration key. Labels are used to group and organize settings.",
        Required = false
    };

    public static readonly Option<string> ContentType = new(
        $"--{ContentTypeName}"
    )
    {
        Description = "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed.",
        Required = false
    };

    public static readonly Option<string[]> Tags = new(
        $"--{TagsName}"
    )
    {
        Description = "The tags to associate with the configuration key. Tags should be in the format 'key=value'. Multiple tags can be specified.",
        Required = false,
        AllowMultipleArgumentsPerToken = true
    };

    public static class KeyValueList
    {
        public static readonly Option<string> Key = new(
            $"--{KeyName}"
        )
        {
            Description = "Specifies the key filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a key of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'App*'). If omitted all keys will be retrieved.",
            Required = false
        };

        public static readonly Option<string> Label = new(
            $"--{LabelName}"
        )
        {
            Description = "Specifies the label filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a label of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'Prod*'). This filter is case-sensitive. If omitted, all labels will be retrieved.",
            Required = false
        };
    }
}
