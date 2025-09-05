// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Services.Models
{
    /// <summary> An ARM Resource SKU. </summary>
    internal sealed class SqlSku
    {
        /// <summary> The name of the SKU, typically, a letter + Number code, e.g. P3. </summary>
        public string? Name { get; set; }
        /// <summary> The tier or edition of the particular SKU, e.g. Basic, Premium. </summary>
        public string? Tier { get; set; }
        /// <summary> Size of the particular SKU. </summary>
        public string? Size { get; set; }
        /// <summary> If the service has different generations of hardware, for the same SKU, then that can be captured here. </summary>
        public string? Family { get; set; }
        /// <summary> Capacity of the particular SKU. </summary>
        public int? Capacity { get; set; }
    }
}
