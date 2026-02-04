// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

internal static class DocumentDbOptionDefinitions
{
    public static readonly Option<string> ConnectionString = new("--connection-string")
    {
        Description = "DocumentDB connection string",
        Required = true
    };

    public static readonly Option<bool> TestConnection = new("--test-connection")
    {
        Description = "Test connection after connecting",
        DefaultValueFactory = _ => true
    };

    public static readonly Option<string> DbName = new("--db-name")
    {
        Description = "Database name"
    };

    public static readonly Option<string> CollectionName = new("--collection-name")
    {
        Description = "Collection name"
    };

    public static readonly Option<string> NewCollectionName = new("--new-collection-name")
    {
        Description = "New collection name"
    };

    public static readonly Option<int> SampleSize = new("--sample-size")
    {
        Description = "Number of documents to sample",
        DefaultValueFactory = _ => 10
    };

    public static readonly Option<string> Query = new("--query")
    {
        Description = "Query filter in JSON format"
    };

    public static readonly Option<string> Options = new("--options")
    {
        Description = "Query options"
    };

    public static readonly Option<string> Document = new("--document")
    {
        Description = "Document to insert"
    };

    public static readonly Option<string> Documents = new("--documents")
    {
        Description = "Documents to insert"
    };

    public static readonly Option<string> Filter = new("--filter")
    {
        Description = "Filter for update/delete"
    };

    public static readonly Option<string> Update = new("--update")
    {
        Description = "Update operations"
    };

    public static readonly Option<bool> Upsert = new("--upsert")
    {
        Description = "Create document if it doesn't exist",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<string> Pipeline = new("--pipeline")
    {
        Description = "Aggregation pipeline"
    };

    public static readonly Option<bool> AllowDiskUse = new("--allow-disk-use")
    {
        Description = "Allow pipeline stages to write to disk",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<string> Keys = new("--keys")
    {
        Description = "Index keys"
    };

    public static readonly Option<string> IndexName = new("--index-name")
    {
        Description = "Index name"
    };

    public static readonly Option<string> Ops = new("--ops")
    {
        Description = "Filter for current operations"
    };
}
