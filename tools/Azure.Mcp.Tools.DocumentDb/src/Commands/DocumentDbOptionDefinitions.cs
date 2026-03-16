// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

internal static class DocumentDbOptionDefinitions
{
    public static readonly Option<string> ConnectionString = new("--connection-string")
    {
        Description = "Azure DocumentDB connection string used for this request.",
        Required = true
    };

    public static readonly Option<string> DbName = new("--db-name")
    {
        Description = "Database name for the requested operation.",
        Required = true
    };

    public static readonly Option<string> CollectionName = new("--collection-name")
    {
        Description = "Collection name for collection, document, or index operations.",
        Required = true
    };

    public static readonly Option<string> ResourceType = CreateResourceTypeOption();

    public static readonly Option<string> NewCollectionName = new("--new-collection-name")
    {
        Description = "New name to assign to the collection.",
        Required = true
    };

    public static readonly Option<int> SampleSize = new("--sample-size")
    {
        Description = "Number of documents to sample from the target collection.",
        DefaultValueFactory = _ => 10
    };

    public static readonly Option<string> Query = new("--query")
    {
        Description = "Query or filter document in JSON format."
    };

    public static readonly Option<string> Options = new("--options")
    {
        Description = "Command-specific options in JSON format."
    };

    public static readonly Option<string> DocumentsPayload = new("--documents")
    {
        Description = "Single JSON document or JSON array of documents to insert.",
        Required = true
    };

    public static readonly Option<string> Filter = new("--filter")
    {
        Description = "Filter document in JSON format for update or delete operations.",
        Required = true
    };

    public static readonly Option<string> Update = new("--update")
    {
        Description = "Update document in JSON format.",
        Required = true
    };

    public static readonly Option<bool> Upsert = new("--upsert")
    {
        Description = "Insert a matching document when an update operation finds no match.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<string> Pipeline = new("--pipeline")
    {
        Description = "Aggregation pipeline in JSON array format.",
        Required = true
    };

    public static readonly Option<bool> AllowDiskUse = new("--allow-disk-use")
    {
        Description = "Allow aggregation stages to use temporary disk space.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<string> Mode = CreateModeOption();

    public static readonly Option<string> Operation = CreateOperationOption();

    public static readonly Option<string> Keys = new("--keys")
    {
        Description = "Index key specification in JSON format.",
        Required = true
    };

    public static readonly Option<string> IndexName = new("--index-name")
    {
        Description = "Index name for the requested operation.",
        Required = true
    };

    public static readonly Option<string> Ops = new("--ops")
    {
        Description = "Current operation filter in JSON format."
    };

    private static Option<string> CreateResourceTypeOption()
    {
        var option = new Option<string>("--resource-type")
        {
            Description = "Resource type to retrieve statistics for. Valid values: collection, database, index.",
            Required = true
        };

        option.AcceptOnlyFromAmong("collection", "database", "index");
        return option;
    }

    private static Option<string> CreateModeOption()
    {
        var option = new Option<string>("--mode")
        {
            Description = "Execution mode. Valid values: single, many. If omitted, command-specific defaults apply.",
            DefaultValueFactory = _ => "single"
        };

        option.AcceptOnlyFromAmong("single", "many");
        return option;
    }

    private static Option<string> CreateOperationOption()
    {
        var option = new Option<string>("--operation")
        {
            Description = "Explain operation. Valid values: find, count, aggregate.",
            Required = true
        };

        option.AcceptOnlyFromAmong("find", "count", "aggregate");
        return option;
    }
}
