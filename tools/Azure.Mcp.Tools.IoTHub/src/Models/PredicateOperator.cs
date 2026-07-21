// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// The comparison operator applied by a <see cref="QueryPredicate"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PredicateOperator>))]
public enum PredicateOperator
{
    /// <summary>Equality comparison (<c>=</c>).</summary>
    Equals,

    /// <summary>Inequality comparison (<c>!=</c>).</summary>
    NotEquals,

    /// <summary>Less-than comparison (<c>&lt;</c>).</summary>
    LessThan,

    /// <summary>Less-than-or-equal comparison (<c>&lt;=</c>).</summary>
    LessThanOrEqual,

    /// <summary>Greater-than comparison (<c>&gt;</c>).</summary>
    GreaterThan,

    /// <summary>Greater-than-or-equal comparison (<c>&gt;=</c>).</summary>
    GreaterThanOrEqual
}
