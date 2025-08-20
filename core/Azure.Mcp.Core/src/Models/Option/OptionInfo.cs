// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Models.Option;

public class OptionInfo(
    string name,
    string description,
    string? value = "",
    string? defaultValue = default,
    bool required = false) :
    OptionDefinition<string>(name, description, value, defaultValue, required);
