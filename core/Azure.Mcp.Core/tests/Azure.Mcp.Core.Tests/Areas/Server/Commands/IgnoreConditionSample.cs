// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

internal sealed record IgnoreConditionSample(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Always)] string AlwaysIgnored,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)] string IgnoredWhenWriting,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)] string IgnoredWhenReading,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] int DefaultValue,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? NullValue,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool NeverIgnored);
