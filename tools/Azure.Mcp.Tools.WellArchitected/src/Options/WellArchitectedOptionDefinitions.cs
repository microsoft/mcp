// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Options;

public static class WellArchitectedOptionDefinitions
{
    public const string PillarName = "pillar";
    public const string ServiceName = "service";
    public const string RecommendationIdName = "recommendation-id";
    public const string InfrastructureConfigName = "infrastructure-config";
    public const string IntentName = "intent";

    public static readonly Option<string> Pillar = new($"--{PillarName}")
    {
        Description = "The Well-Architected Framework pillar (Security, Reliability, Performance, Cost, Operational Excellence)"
    };

    public static readonly Option<string> Service = new($"--{ServiceName}")
    {
        Description = "The Azure service name for filtering recommendations"
    };

    public static readonly Option<string> RecommendationId = new($"--{RecommendationIdName}")
    {
        Description = "The recommendation ID (e.g., SE:01, RE:02)"
    };

    public static readonly Option<string> InfrastructureConfig = new($"--{InfrastructureConfigName}")
    {
        Description = "The infrastructure configuration in JSON format (ARM template, Bicep compiled output, or resource list)"
    };

    public static readonly Option<string> Intent = new($"--{IntentName}")
    {
        Description = "The intended purpose or workload description for the infrastructure"
    };
}
