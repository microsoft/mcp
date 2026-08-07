// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Services.Models;

/// <summary>
/// A class representing the SqlServerAadAdministrator properties model.
/// Azure Active Directory administrator properties.
/// </summary>
internal sealed class SqlServerAadAdministratorProperties
{
    /// <summary> Type of the sever administrator. </summary>
    public string? AdministratorType { get; set; }
    /// <summary> Login name of the server administrator. </summary>
    public string? Login { get; set; }
    /// <summary> SID (object ID) of the server administrator. </summary>
    public Guid? Sid { get; set; }
    /// <summary> Tenant ID of the administrator. </summary>
    public Guid? TenantId { get; set; }
    /// <summary> Azure Active Directory only Authentication enabled. </summary>
    [JsonPropertyName("azureADOnlyAuthentication")]
    public bool? IsAzureADOnlyAuthenticationEnabled { get; set; }
}
