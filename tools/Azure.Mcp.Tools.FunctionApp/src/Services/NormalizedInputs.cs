// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Trimmed and lower-cased create inputs after validation.
/// </summary>
public readonly record struct NormalizedInputs(
    string Runtime,
    string? RuntimeVersion,
    string? PlanType,
    string? PlanSku,
    string? OperatingSystem,
    string? StorageAccountName,
    string? ContainerAppsEnvironmentName);
