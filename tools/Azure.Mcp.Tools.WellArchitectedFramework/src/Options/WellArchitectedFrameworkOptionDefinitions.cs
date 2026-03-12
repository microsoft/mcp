// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options;

public static class WellArchitectedFrameworkOptionDefinitions
{
    public const string ServiceName = "service";
    public const string ServicesName = "services";

    private const string ServiceNameFormat =
        "case-insensitive; hyphens, underscores, spaces, and name variations allowed; use double quotes (not single quotes) for names with spaces. " +
        """e.g., cosmos-db, Cosmos_DB, "Cosmos DB", cosmosdb, cosmos-database, cosmosdatabase""";

    public const string ServiceNameDescription =
        "The Azure service name. Service name format: " + ServiceNameFormat;

    public const string ServicesNameDescription =
        "A list of Azure service names. Multiple services must be space separated, not comma separated. " +
        """e.g., --services sql-database "App Service" Cosmos-DB """ +
        "Each Azure service name format: " + ServiceNameFormat;

    public static readonly Option<string> Service = new($"--{ServiceName}", "-s")
    {
        Description = ServiceNameDescription,
        Required = true
    };

    public static readonly Option<string[]> Services = new($"--{ServicesName}", "-s")
    {
        Description = ServicesNameDescription,
        Required = true,
        AllowMultipleArgumentsPerToken = true
    };
}
