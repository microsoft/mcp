// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options.Database;

public class DatabaseDeleteOptions : BaseDatabaseOptions
{
    // Delete operations only need the base database properties: Server, Database, ResourceGroup, Subscription
    // No additional options are needed for database deletion
}
