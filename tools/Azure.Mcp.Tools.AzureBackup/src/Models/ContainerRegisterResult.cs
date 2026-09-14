// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ContainerRegisterResult(
    string Status,
    RegisteredContainerInfo Container,
    bool AlreadyRegistered,
    string Message);

public sealed record RegisteredContainerInfo(
    string Name,
    string? FriendlyName,
    string? BackupManagementType,
    string? RegistrationStatus,
    string? HealthStatus,
    string? SourceResourceId);
