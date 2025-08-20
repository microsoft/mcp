// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Models.Option;

public class OptionDefinition<T>(string name, string description, string? value = "", T? defaultValue = default, bool required = false, bool hidden = false)
    where T : notnull
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = name;

    [JsonPropertyName("description")]
    public string Description { get; set; } = description;

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string Value { get; set; } = string.IsNullOrEmpty(value) ? null! : value;

    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? DefaultValue { get; set; } = defaultValue;

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; } = typeof(T).Name.ToLowerInvariant();

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Required { get; set; } = required;

    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Hidden { get; set; } = hidden;

    public Option<T> ToOption()
    {
        var option = new Option<T>($"--{Name}", Description);

        if (DefaultValue != null)
        {
            option.SetDefaultValue(DefaultValue);
        }
        option.IsRequired = Required;
        option.IsHidden = Hidden;
        return option;
    }

    public JsonPropertyNameAttribute ToJsonAttribute()
    {
        return new JsonPropertyNameAttribute(Name);
    }
}
