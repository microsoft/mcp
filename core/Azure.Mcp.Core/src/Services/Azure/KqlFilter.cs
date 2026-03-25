// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Represents a single KQL filter condition used in Resource Graph queries.
/// The base class constructs the KQL fragment by escaping both the field identifier
/// and the value, so callers never concatenate raw strings into the query.
/// </summary>
/// <param name="Field">The KQL field/column name to filter on (e.g. "name", "id").</param>
/// <param name="Operator">The KQL comparison operator (e.g. "=~", "contains", "==").</param>
/// <param name="Value">The value to compare against. Will be escaped as a KQL string literal.</param>
public sealed record KqlFilter(string Field, string Operator, string Value);
