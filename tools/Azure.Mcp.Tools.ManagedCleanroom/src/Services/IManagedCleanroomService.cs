// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

/// <summary>
/// Data-plane operations against the Cleanroom Analytics Frontend service.
/// Authentication uses a bearer token scoped to the frontend endpoint.
/// </summary>
public interface IManagedCleanroomServiceDataPlane
{
    Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        string? tokenScope = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Control-plane (ARM) operations for managing Cleanroom collaboration resources.
/// Authentication uses the standard Azure Resource Manager credential.
/// </summary>
public interface IManagedCleanroomServiceControlPlane
{
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

/// <summary>Result returned by <see cref="IManagedCleanroomServiceControlPlane.CreateCollaborationArmResourceAsync"/>.</summary>
/// <param name="Properties">ARM resource properties as a raw <see cref="System.Text.Json.JsonElement"/>.</param>
/// <param name="Message">Human-readable summary of the provisioning outcome including elapsed time.</param>
public sealed record CollaborationCreateResult(System.Text.Json.JsonElement Properties, string Message);
