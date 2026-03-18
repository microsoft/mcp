// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Services;

/// <summary>
/// Service interface for managing Cosmos DB container copy jobs
/// via the ARM copyJobs API (Microsoft.DocumentDB/databaseAccounts/copyJobs).
/// </summary>
public interface ICopyJobService
{
    /// <summary>Creates a new copy job on the specified account.</summary>
    Task<JsonElement> CreateJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string jobPropertiesJson,
        string? mode = null,
        int? workerCount = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the status of an existing copy job.</summary>
    Task<JsonElement> GetJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists all copy jobs on the specified account.</summary>
    Task<List<JsonElement>> ListJobsAsync(
        string subscription,
        string accountName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels a running or pending copy job.</summary>
    Task<JsonElement> CancelJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Pauses a running copy job.</summary>
    Task<JsonElement> PauseJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resumes a paused copy job.</summary>
    Task<JsonElement> ResumeJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Completes an Online copy job (flushes remaining changes).</summary>
    Task<JsonElement> CompleteJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
