// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService : IDisposable
{
    // Connection Management
    Task<object> ConnectAsync(string connectionString, bool testConnection = true, CancellationToken cancellationToken = default);
    Task<object> DisconnectAsync(CancellationToken cancellationToken = default);
    Task<object> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
}
