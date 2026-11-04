// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal sealed class MicrosoftIdentityApplicationConfiguration()
{
    public string? Name { get; set; }
    public string? Instance { get; set; }
    public string? TenantId { get; set; }
    public string? AppHomeTenantId { get; set; }
    public string? ClientId { get; set; }
    public string? Authority { get; set; }
    public string? AzureRegion { get; set; }
    public string? Audience { get; set; }
    public List<string>? Audiences { get; set; }
    public List<string>? ClientCapabilities { get; set; }
    public List<CredentialConfiguration>? ClientCredentials { get; set; }
    public List<CredentialConfiguration>? TokenDecryptionCredentials { get; set; }
    public Dictionary<string, string>? ExtraQueryParameters { get; set; }
    public bool? AllowWebApiToBeAuthorizedByACL { get; set; }
    public bool? EnablePiiLogging { get; set; }
    public bool? SendX5C { get; set; }
    public bool? WithSpaAuthCode { get; set; }
    public string? Domain { get; set; }
    public string? EditProfilePolicyId { get; set; }
    public string? SignUpSignInPolicyId { get; set; }
    public string? ResetPasswordPolicyId { get; set; }
    public string? ResetPasswordPath { get; set; }
    public string? ErrorPath { get; set; }
}
