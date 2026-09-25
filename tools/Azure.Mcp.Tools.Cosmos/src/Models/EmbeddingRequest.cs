// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Models;

/// <summary>
/// Configuration required to generate an embedding from free-form text via Azure OpenAI.
/// </summary>
/// <param name="Endpoint">Azure OpenAI endpoint, e.g., "https://my-endpoint.openai.azure.com/".</param>
/// <param name="DeploymentName">Name of the embedding deployment.</param>
/// <param name="Dimensions">
/// Optional embedding dimensions to request. Only honored by models that support custom dimensions
/// (for example, <c>text-embedding-3-*</c>). When <c>null</c>, the model's native dimensionality is used.
/// </param>
public sealed record EmbeddingRequest(string Endpoint, string DeploymentName, int? Dimensions);
