// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// A projection of a Platform Landing Zone ARM resource.
/// </summary>
/// <remarks>
/// The ARM resource operation and the generation run complete independently:
/// <see cref="ProvisioningState"/> is terminal as soon as the resource is persisted, while
/// <see cref="Status"/> tracks the asynchronous generation run and is what callers poll.
/// </remarks>
/// <param name="Name">The Platform Landing Zone resource name.</param>
/// <param name="ProvisioningState">The ARM provisioning state of the resource itself.</param>
/// <param name="Status">The generation run status: Running, Succeeded or Failed.</param>
/// <param name="ArtifactId">
/// The resource ID of the artifact holding the generated output. Deterministic, so it is present even
/// while generation is still running.
/// </param>
/// <param name="EffectiveProperties">
/// The service's echo of the full effective configuration, including every value the service defaulted
/// on the caller's behalf, as indented JSON.
/// </param>
public sealed record PlatformLandingZoneView(
    string? Name,
    string? ProvisioningState,
    string? Status,
    string? ArtifactId,
    string? EffectiveProperties);
