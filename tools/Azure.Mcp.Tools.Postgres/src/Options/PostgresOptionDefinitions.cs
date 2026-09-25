// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Postgres.Options;

public static class PostgresOptionDefinitions
{
    public const string AuthTypeText = "auth-type";

    internal const string AuthTypeDescription = $"The authentication type to access PostgreSQL server. " +
        $"Supported values are '{AuthTypes.MicrosoftEntra}' or '{AuthTypes.PostgreSQL}'. By default '{AuthTypes.MicrosoftEntra}' is used.";
    internal const string UserDescription = "The user name to access PostgreSQL server.";
    internal const string PasswordDescription = $"The user password to access PostgreSQL server, Only required if '{AuthTypeText}' is set to '{AuthTypes.PostgreSQL}' authentication, not needed for '{AuthTypes.MicrosoftEntra}' authentication.";
    internal const string ServerDescription = "The PostgreSQL server to be accessed.";
    internal const string DatabaseDescription = "The PostgreSQL database to be accessed.";
    internal const string ParamDescription = "The PostgreSQL parameter to be accessed.";
}
