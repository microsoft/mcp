// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Models;

/// <summary>
/// Approximate schema inferred for a Cosmos DB container.
/// </summary>
/// <param name="SampleSize">Number of documents that were sampled.</param>
/// <param name="Properties">Top-level properties discovered across the sampled documents.</param>
public sealed record ContainerSchema(int SampleSize, IReadOnlyList<SchemaProperty> Properties);
