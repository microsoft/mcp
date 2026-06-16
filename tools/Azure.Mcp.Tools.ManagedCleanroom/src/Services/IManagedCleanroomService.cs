// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public interface IManagedCleanroomService
{
    Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        bool allowUntrustedCert = false,
        string? tokenScope = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<CollaborationCreateResult> CreateCollaborationArmResourceAsync(
        string name,
        string resourceGroup,
        string subscription,
        string location,
        string? resourceLocation = null,
        string[]? collaborators = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Result returned by <see cref="IManagedCleanroomService.CreateCollaborationArmResourceAsync"/>.</summary>
/// <param name="Properties">ARM resource properties as a raw <see cref="System.Text.Json.JsonElement"/>.</param>
/// <param name="Message">Human-readable summary of the provisioning outcome including elapsed time.</param>
public sealed record CollaborationCreateResult(System.Text.Json.JsonElement Properties, string Message);
