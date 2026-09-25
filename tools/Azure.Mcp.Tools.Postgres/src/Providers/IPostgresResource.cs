// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Npgsql;

namespace Azure.Mcp.Tools.Postgres.Providers;

public interface IPostgresResource : IAsyncDisposable
{
    NpgsqlConnection Connection { get; }
}
