// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options;

public static class WellArchitectedFrameworkOptionDefinitions
{
    public const string ServiceName = "service";

    public const string ServiceNameDescription =
        "A single Azure service name. Service name format: case-insensitive; hyphens, underscores, spaces, and name variations allowed; " +
        "use double quotes (not single quotes) for names with spaces. " +
        """e.g., cosmos-db, Cosmos_DB, "Cosmos DB", cosmosdb, cosmos-database, cosmosdatabase""";

    public static readonly Option<string> Service = new($"--{ServiceName}", "-s")
    {
        Description = ServiceNameDescription,
        Required = false
    };
}
