// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

    public async Task<object> ConnectAsync(string connectionString, bool testConnection = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new Dictionary<string, object?>
                {
                    ["success"] = false,
                    ["statusCode"] = HttpStatusCode.BadRequest,
                    ["message"] = "Connection string cannot be empty",
                    ["data"] = null
                };
            }

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

                return new Dictionary<string, object?>
                {
                    ["success"] = true,
                    ["statusCode"] = HttpStatusCode.OK,
                    ["message"] = "Connected successfully",
                    ["data"] = new Dictionary<string, object?>
                    {
                        ["databaseCount"] = databases.Count,
                        ["databases"] = databases
                    }
                };
            }

            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Connected successfully (not tested)",
                ["data"] = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to DocumentDB");
            _client = null;
            _connectionString = null;
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = $"Connection failed: {ex.Message}",
                ["data"] = null
            };
        }
    }

    public Task<object> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_client == null)
            {
                return Task.FromResult<object>(new Dictionary<string, object?>
                {
                    ["success"] = true,
                    ["statusCode"] = HttpStatusCode.OK,
                    ["message"] = "No active connection",
                    ["data"] = new Dictionary<string, object?>
                    {
                        ["isConnected"] = false
                    }
                });
            }

            _client = null;
            _connectionString = null;
            _logger.LogInformation("Disconnected from DocumentDB");
            return Task.FromResult<object>(new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Disconnected successfully",
                ["data"] = new Dictionary<string, object?>
                {
                    ["isConnected"] = false
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
            return Task.FromResult<object>(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = $"Disconnect failed: {ex.Message}",
                ["data"] = null
            });
        }
    }

    public async Task<object> GetConnectionStatusAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Not connected",
                ["data"] = new Dictionary<string, object?>
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

            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Connection status retrieved successfully",
                ["data"] = new Dictionary<string, object?>
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
            _logger.LogWarning(ex, "Connection status check failed");
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = $"Failed to check connection status: {ex.Message}",
                ["data"] = null
            };
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

    #region Helper Methods

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
