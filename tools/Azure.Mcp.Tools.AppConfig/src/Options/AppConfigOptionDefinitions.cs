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
        $"--{AccountName}",
        "The name of the App Configuration store (e.g., my-appconfig)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Key = new(
        $"--{KeyName}",
        "The name of the key to access within the App Configuration store."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Value = new(
        $"--{ValueName}",
        "The value to set for the configuration key."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Label = new(
        $"--{LabelName}",
        "The label to apply to the configuration key. Labels are used to group and organize settings."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> ContentType = new(
        $"--{ContentTypeName}",
        "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string[]> Tags = new(
        $"--{TagsName}",
        "The tags to associate with the configuration key. Tags should be in the format 'key=value'. Multiple tags can be specified."
    )
    {
        IsRequired = false,
        AllowMultipleArgumentsPerToken = true
    };

    public static class KeyValueList
    {
        public static readonly Option<string> Key = new(
            $"--{KeyName}",
            "Specifies the key filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a key of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'App*'). If omitted all keys will be retrieved."
        )
        {
            IsRequired = false
        };

        public static readonly Option<string> Label = new(
            $"--{LabelName}",
            "Specifies the label filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a label of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'Prod*'). This filter is case-sensitive. If omitted, all labels will be retrieved."
        )
        {
            IsRequired = false
        };
    }
}
