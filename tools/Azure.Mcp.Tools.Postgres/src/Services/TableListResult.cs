// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Postgres.Services;

public sealed record TableListResult(List<string> Tables, bool IsTruncated);
