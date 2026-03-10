// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DocumentDb.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService : IDisposable
{
    // Connection Management
    Task<DocumentDbResponse> ConnectAsync(string connectionString, bool testConnection = true, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DisconnectAsync(CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
}
