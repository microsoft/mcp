// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

// Connection Options
public class ConnectDocumentDbOptions : BaseDocumentDbOptions
{
    public string? ConnectionString { get; set; }
    public bool TestConnection { get; set; } = true;
}

public class DisconnectDocumentDbOptions : BaseDocumentDbOptions;

public class GetConnectionStatusOptions : BaseDocumentDbOptions;

// Database Options
public class ListDatabasesOptions : BaseDocumentDbOptions;

public class DbStatsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
}

public class GetDbInfoOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
}

public class DropDatabaseOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
}

// Collection Options
public class CollectionStatsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
}

public class RenameCollectionOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? NewCollectionName { get; set; }
}

public class DropCollectionOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
}

public class SampleDocumentsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public int SampleSize { get; set; } = 10;
}

// Document Options
public class FindDocumentsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Query { get; set; }
    public string? Options { get; set; }
}

public class CountDocumentsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Query { get; set; }
}

public class InsertDocumentOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Document { get; set; }
}

public class InsertManyOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Documents { get; set; }
}

public class UpdateDocumentOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Filter { get; set; }
    public string? Update { get; set; }
    public bool Upsert { get; set; }
}

public class UpdateManyOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Filter { get; set; }
    public string? Update { get; set; }
    public bool Upsert { get; set; }
}

public class DeleteDocumentOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Filter { get; set; }
}

public class DeleteManyOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Filter { get; set; }
}

public class AggregateOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Pipeline { get; set; }
    public bool AllowDiskUse { get; set; }
}

public class FindAndModifyOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Query { get; set; }
    public string? Update { get; set; }
    public bool Upsert { get; set; }
}

public class ExplainFindQueryOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Query { get; set; }
    public string? Options { get; set; }
}

public class ExplainCountQueryOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Query { get; set; }
}

public class ExplainAggregateQueryOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Pipeline { get; set; }
}

// Index Options
public class CreateIndexOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? Keys { get; set; }
    public string? Options { get; set; }
}

public class ListIndexesOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
}

public class DropIndexOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
    public string? IndexName { get; set; }
}

public class IndexStatsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }
    public string? CollectionName { get; set; }
}

public class CurrentOpsOptions : BaseDocumentDbOptions
{
    public string? Ops { get; set; }
}
