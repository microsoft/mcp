// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class CatalogSearchOptions
{
    [Option(Description = "The text query for the search. Supports searching across display name, description and workspace name of the catalog entry.")]
    public string? Search { get; set; }

    [Option(Description = "The filter for the search. Supports filtering by type of entries. Supported operators: eq (Equals), ne (Not Equals), or (Logical OR), () (Parentheses for grouping). Example: \"Type eq 'Report' or Type eq 'Lakehouse'\". Supported item types: Dashboard, Report, SemanticModel, PaginatedReport, Lakehouse, Eventhouse, Environment, KQLDatabase, KQLQueryset, KQLDashboard, DataPipeline, Notebook, SparkJobDefinition, MLExperiment, MLModel, Warehouse, Eventstream, SQLEndpoint, MirroredWarehouse, MirroredDatabase, Reflex, GraphQLApi, MountedDataFactory, SQLDatabase, CopyJob, VariableLibrary, Dataflow, ApacheAirflowJob, WarehouseSnapshot, DigitalTwinBuilder, DigitalTwinBuilderFlow, MirroredAzureDatabricksCatalog, Map, AnomalyDetector, UserDataFunction, GraphModel, GraphQuerySet, SnowflakeDatabase, OperationsAgent, CosmosDBDatabase, Ontology, EventSchemaSet (additional types may be added over time).")]
    public string? Filter { get; set; }

    [Option(Description = "The page size that needs to be returned. Must be between 1 and 1000.")]
    public int? PageSize { get; set; }

    [Option(Description = "A token for retrieving the next page of results.")]
    public string? ContinuationToken { get; set; }
}
