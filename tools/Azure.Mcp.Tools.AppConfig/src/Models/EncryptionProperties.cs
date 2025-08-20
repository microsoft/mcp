// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppConfig.Models;

public class EncryptionProperties
{
    public string? KeyIdentifier { get; set; }
    public string? IdentityClientId { get; set; }
    public bool? IsKeyVaultKeyIdentifierValid { get; set; }
    public bool? IsIdentityClientIdValid { get; set; }
}
