// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.KeyVault.Models;
using Azure.Mcp.Tools.KeyVault.Options.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.KeyVault.Commands.Certificate;

[CommandMetadata(
    Id = "4ae12e3e-dee0-4d8d-ad34-ffeaf70c642b",
    Name = "import",
    Title = "Import Key Vault Certificate",
    Description = "Imports/uploads an existing certificate (PFX or PEM with private key) into an Azure Key Vault without generating a new certificate or key material. This command accepts either a file path to a PFX/PEM file, a base64 encoded PFX, or raw PEM text starting with -----BEGIN. If the certificate is a password-protected PFX, a password must be provided. Required: --vault <vault>, --certificate <certificate>, --certificate-data <certificate-data>, --subscription <subscription>. Optional: --password <password-for-PFX>, --tenant <tenant>. Returns: name, id, keyId, secretId, cer (base64), thumbprint, enabled, notBefore, expiresOn, createdOn, updatedOn, subject, issuer. Creates a new certificate version if it already exists.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class CertificateImportCommand(ILogger<CertificateImportCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CertificateImportOptions, CertificateDetails>(subscriptionResolver)
{
    private readonly ILogger<CertificateImportCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CertificateImportOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _keyVaultService.ImportCertificate(
                options.Vault,
                options.Certificate,
                options.CertificateData,
                options.Password,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(CertificateDetails.FromCertificate(certificate), KeyVaultJsonContext.Default.CertificateDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing certificate {CertificateName} into vault {VaultName}", options.Certificate, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
