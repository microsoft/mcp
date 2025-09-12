// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Services.Models
{
    /// <summary>
    /// A class representing the SqlDatabase properties model.
    /// A database resource properties.
    /// </summary>
    internal sealed class SqlDatabaseProperties
    {
        /// <summary> The collation of the database. </summary>
        public string? Collation { get; set; }
        /// <summary> The max size of the database expressed in bytes. </summary>
        public long? MaxSizeBytes { get; set; }
        /// <summary> The resource identifier of the elastic pool containing this database. </summary>
        public string? ElasticPoolId { get; set; }
        /// <summary> The status of the database. </summary>
        public string? Status { get; set; }
        /// <summary> The creation date of the database (ISO8601 format). </summary>
        [JsonPropertyName("creationDate")]
        public DateTimeOffset? CreatedOn { get; set; }
        /// <summary> The current service level objective name of the database. </summary>
        public string? CurrentServiceObjectiveName { get; set; }
        /// <summary> This records the earliest start date and time that restore is available for this database (ISO8601 format). </summary>
        [JsonPropertyName("earliestRestoreDate")]
        public DateTimeOffset? EarliestRestoreOn { get; set; }
        /// <summary> The state of read-only routing. If enabled, connections that have application intent set to readonly in their connection string may be routed to a readonly secondary replica in the same region. Not applicable to a Hyperscale database within an elastic pool. </summary>
        public string? ReadScale { get; set; }
        /// <summary> Whether or not this database is zone redundant, which means the replicas of this database will be spread across multiple availability zones. </summary>
        public bool? IsZoneRedundant { get; set; }
        /// <summary> The name and tier of the SKU. </summary>
        public SqlSku? CurrentSku { get; set; }
    }
}
