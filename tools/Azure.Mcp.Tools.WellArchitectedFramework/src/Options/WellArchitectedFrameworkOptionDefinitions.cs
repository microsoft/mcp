// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options;

public static class WellArchitectedFrameworkOptionDefinitions
{
    public const string ServiceName = "service";

    public static readonly Option<string> Service = new($"--{ServiceName}", "-s")
    {
        Description = "The name of the Azure service to get Well-Architected Framework guidance for (e.g., 'app-service', 'storage', 'sql-database', 'cosmos-db'). Use lowercase with hyphens.",
        Required = true
    };
}
