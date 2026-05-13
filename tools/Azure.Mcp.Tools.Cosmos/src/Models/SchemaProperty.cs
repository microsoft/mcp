// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Models;

/// <summary>
/// Approximate schema inferred from a sample of documents in a Cosmos DB container.
/// </summary>
/// <param name="Name">Property name as it appears on the document.</param>
/// <param name="Type">Pipe-delimited list of JSON value kinds observed (e.g., "string" or "number | null").</param>
/// <param name="AppearedIn">Number of sampled documents that contained this property.</param>
/// <param name="SampleSize">Total number of sampled documents.</param>
public sealed record SchemaProperty(string Name, string Type, int AppearedIn, int SampleSize);
