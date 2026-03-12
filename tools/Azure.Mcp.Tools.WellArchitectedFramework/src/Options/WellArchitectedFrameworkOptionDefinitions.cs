// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options;

public static class WellArchitectedFrameworkOptionDefinitions
{
    public const string ServiceName = "service";

    public static readonly Option<string> Service = new($"--{ServiceName}", "-s")
    {
        Description = "The Azure service name (case-insensitive; spaces and hyphens are normalized) " +
                      "e.g., 'App Service', 'app-service', 'SQL Database', 'sql-database', 'Cosmos DB', 'cosmos-db'.",
        Required = true
    };
}
