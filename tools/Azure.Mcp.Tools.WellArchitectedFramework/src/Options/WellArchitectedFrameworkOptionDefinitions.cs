// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options;

public static class WellArchitectedFrameworkOptionDefinitions
{
    public const string ServiceName = "service";
    public const string ModeName = "mode";

    public const string ServiceNameDescription =
        "A single Azure service name. Service name format: case-insensitive; hyphens, underscores, spaces, and name variations allowed; " +
        "use double quotes (not single quotes) for names with spaces. " +
        """e.g., cosmos-db, Cosmos_DB, "Cosmos DB", cosmosdb, cosmos-database, cosmosdatabase""";

    public const string ModeDescription =
        "Output mode for service guide. 'url' returns URL to markdown file, 'summary' returns JSON content with service guide summary. Default: summary";

    public static readonly Option<string> Service = new($"--{ServiceName}", "-s")
    {
        Description = ServiceNameDescription,
        Required = false
    };

    public static readonly Option<string> Mode = new($"--{ModeName}", "-m")
    {
        Description = ModeDescription,
        Required = false
    };
}
