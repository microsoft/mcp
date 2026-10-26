// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Thrown when a vault was created successfully but its requested storage redundancy could not be applied.
/// Preserves the Azure status and error code so callers can distinguish this partial success from
/// an initial vault-creation failure (for example, a 409 for an existing vault).
/// </summary>
public sealed class VaultStorageRedundancyException(int status, string message, string? errorCode, Exception? innerException)
    : RequestFailedException(status, message, errorCode, innerException);
