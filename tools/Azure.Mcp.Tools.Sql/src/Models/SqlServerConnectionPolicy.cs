// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Models;

public record SqlServerConnectionPolicy(
    string Name,
    string Id,
    string Type,
    string ConnectionType);
