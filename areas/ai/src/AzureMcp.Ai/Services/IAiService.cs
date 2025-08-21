// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Options;
using AzureMcp.Ai.Models;

namespace AzureMcp.Ai.Services;

public interface IAiService
{
    Task<CompletionResult> CreateCompletionAsync(
        string resourceName,
        string deploymentName,
        string promptText,
        string subscription,
        string resourceGroup,
        int? maxTokens = null,
        double? temperature = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
