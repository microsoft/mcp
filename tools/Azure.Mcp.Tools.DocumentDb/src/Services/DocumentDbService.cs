// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.DocumentDb.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public class DocumentDbService(ILogger<DocumentDbService> logger) : IDocumentDbService
{
    private readonly ILogger<DocumentDbService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private MongoClient? _client;
    private string? _connectionString;
    private bool _disposed;

    /// <summary>
    /// Helper method to convert BsonDocument to JSON string for serialization
    /// </summary>
    private static string? BsonDocumentToJson(BsonDocument? doc)
    {
        return doc?.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson });
    }

    /// <summary>
    /// Helper method to convert List of BsonDocument to List of JSON strings for serialization
    /// </summary>
    private static List<string> BsonDocumentListToJson(List<BsonDocument> docs)
    {
        return docs.Select(doc => doc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson })).ToList();
    }

    #region Connection Management

    public async Task<DocumentDbResponse> ConnectAsync(string connectionString, bool testConnection = true, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));

        try
        {
            // Disconnect any existing connection
            if (_client != null)
            {
                await DisconnectAsync(cancellationToken);
            }

            _connectionString = connectionString;
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            _client = new MongoClient(settings);

            if (testConnection)
            {
                // Test the connection by listing databases
                var databases = await _client.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);
                _logger.LogInformation("Successfully connected to DocumentDB. Found {Count} databases", databases.Count);

                return new DocumentDbResponse
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Connected successfully",
                    Data = new Dictionary<string, object?>
                    {
                        ["databaseCount"] = databases.Count,
                        ["databases"] = databases
                    }
                };
            }

            return new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Connected successfully (not tested)",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _client = null;
            _connectionString = null;

            throw new InvalidOperationException($"Connection failed: {ex.Message}", ex);
        }
    }

    public Task<DocumentDbResponse> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            return Task.FromResult(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "No active connection",
                Data = new Dictionary<string, object?>
                {
                    ["isConnected"] = false
                }
            });
        }

        _client = null;
        _connectionString = null;
        _logger.LogInformation("Disconnected from DocumentDB");
        return Task.FromResult(new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Disconnected successfully",
            Data = new Dictionary<string, object?>
            {
                ["isConnected"] = false
            }
        });
    }

    public async Task<DocumentDbResponse> GetConnectionStatusAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            return new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Not connected",
                Data = new Dictionary<string, object?>
                {
                    ["isConnected"] = false,
                    ["connectionString"] = null,
                    ["details"] = null
                }
            };
        }

        var sanitizedConnectionString = SanitizeConnectionString(_connectionString);

        try
        {
            var adminDb = _client.GetDatabase("admin");
            var command = new BsonDocument("hello", 1);
            await adminDb.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Connection status retrieved successfully",
                Data = new Dictionary<string, object?>
                {
                    ["isConnected"] = true,
                    ["connectionString"] = sanitizedConnectionString,
                    ["details"] = new Dictionary<string, object?>
                    {
                        ["status"] = "Connected and verified"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check connection status: {ex.Message}", ex);
        }
    }

    private string? SanitizeConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        // Hide password from connection string
        try
        {
            var uri = new Uri(connectionString);
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var sanitized = connectionString.Replace(uri.UserInfo, "***:***");
                return sanitized;
            }
        }
        catch
        {
            // If parsing fails, just return a placeholder
            return "mongodb://***";
        }

        return connectionString;
    }

    #endregion

    #region Database Management

    public async Task<DocumentDbResponse> GetDatabasesAsync(string? dbName = null, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var databaseNames = await _client!.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(dbName) && !databaseNames.Contains(dbName, StringComparer.Ordinal))
        {
            return new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            };
        }

        List<Dictionary<string, object?>> databases;
        if (string.IsNullOrWhiteSpace(dbName))
        {
            databases = databaseNames
                .Select(databaseName => new Dictionary<string, object?>
                {
                    ["name"] = databaseName
                })
                .ToList();
        }
        else
        {
            databases = [await GetDatabaseInfoAsync(dbName, cancellationToken)];
        }

        return new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = string.IsNullOrWhiteSpace(dbName)
                ? "Databases retrieved successfully."
                : $"Database '{dbName}' retrieved successfully.",
            Data = databases
        };
    }

    public async Task<DocumentDbResponse> GetDatabaseStatsAsync(string dbName, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        ValidateParameter(dbName, nameof(dbName));

        if (!await DatabaseExistsAsync(dbName, cancellationToken))
        {
            return new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            };
        }

        var database = _client!.GetDatabase(dbName);
        var stats = await database.RunCommandAsync<BsonDocument>(new BsonDocument("dbStats", 1), cancellationToken: cancellationToken);

        return new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = $"Database statistics for '{dbName}' retrieved successfully.",
            Data = stats
        };
    }

    public async Task<DocumentDbResponse> DropDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        ValidateParameter(dbName, nameof(dbName));

        if (!await DatabaseExistsAsync(dbName, cancellationToken))
        {
            return new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            };
        }

        await _client!.DropDatabaseAsync(dbName, cancellationToken);

        _logger.LogInformation("Dropped DocumentDB database {DatabaseName}", dbName);

        return new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = $"Database '{dbName}' dropped successfully.",
            Data = new Dictionary<string, object?>
            {
                ["name"] = dbName,
                ["deleted"] = true
            }
        };
    }

    #endregion

    #region Helper Methods

    private async Task<bool> DatabaseExistsAsync(string dbName, CancellationToken cancellationToken)
    {
        var databaseNames = await _client!.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);
        return databaseNames.Contains(dbName, StringComparer.Ordinal);
    }

    private async Task<Dictionary<string, object?>> GetDatabaseInfoAsync(string dbName, CancellationToken cancellationToken)
    {
        var database = _client!.GetDatabase(dbName);
        var collectionNames = await database.ListCollectionNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        var collections = new List<Dictionary<string, object?>>(collectionNames.Count);
        foreach (var collectionName in collectionNames)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var documentCount = await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken);

            collections.Add(new Dictionary<string, object?>
            {
                ["name"] = collectionName,
                ["documentCount"] = documentCount
            });
        }

        return new Dictionary<string, object?>
        {
            ["name"] = dbName,
            ["collectionCount"] = collectionNames.Count,
            ["collections"] = collections
        };
    }

    private void EnsureConnected()
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Not connected to DocumentDB. Please call ConnectAsync first.");
        }
    }

    private void ValidateParameter(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
            return;

        _client = null;
        _connectionString = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    #endregion
}
