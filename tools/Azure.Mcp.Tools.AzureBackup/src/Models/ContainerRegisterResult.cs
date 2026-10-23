// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ContainerRegisterResult(
    string Status,
    RegisteredContainerInfo Container,
    bool AlreadyRegistered,
    string Message);
