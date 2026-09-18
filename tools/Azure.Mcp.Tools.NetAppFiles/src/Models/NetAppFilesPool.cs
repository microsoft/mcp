// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesPool(
    string Name,
    string Id,
    string Location,
    long SizeInBytes,
    string ServiceLevel,
    string? QosType,
    bool? CoolAccessEnabled,
    string? EncryptionType,
    int? CustomThroughputMibps,
    IReadOnlyDictionary<string, string> Tags,
    string? ProvisioningState);
