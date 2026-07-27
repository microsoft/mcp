// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Postgres.Services;

public sealed record DatabaseListResult(List<string> Databases, bool IsTruncated);
