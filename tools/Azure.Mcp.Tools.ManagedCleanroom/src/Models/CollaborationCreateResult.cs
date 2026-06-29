// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.ManagedCleanroom.Models;

/// <summary>Result returned by <see cref="Azure.Mcp.Tools.ManagedCleanroom.Services.IManagedCleanroomServiceControlPlane.CreateCollaborationArmResourceAsync"/>.</summary>
/// <param name="Properties">ARM resource properties as a raw <see cref="JsonElement"/>.</param>
/// <param name="Message">Human-readable summary of the provisioning outcome including elapsed time.</param>
public sealed record CollaborationCreateResult(JsonElement Properties, string Message);