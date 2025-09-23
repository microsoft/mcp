// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue.Lock;

public class KeyValueLockSetOptions : BaseKeyValueOptions
{
    [JsonPropertyName(AppConfigOptionDefinitions.LockName)]
    public bool Lock { get; set; }
}
