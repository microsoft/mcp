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
    Id = "a11e024a-62e6-4237-8d7d-4b9b8439f50e",
    Name = "create",
    Title = "Create Key Vault Certificate",
    Description = "Create/issue/generate a new certificate in an Azure Key Vault using the default certificate policy. Required: --vault, --certificate, --subscription. Optional: --tenant <tenant>. Returns: name, id, keyId, secretId, cer (base64), thumbprint, enabled, notBefore, expiresOn, createdOn, updatedOn, subject, issuerName. Creates a new certificate version if it already exists.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CertificateCreateCommand(ILogger<CertificateCreateCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CertificateCreateOptions, CertificateDetails>(subscriptionResolver)
{
    private readonly ILogger<CertificateCreateCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CertificateCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _keyVaultService.CreateCertificate(
                options.Vault,
                options.Certificate,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(CertificateDetails.FromCertificate(certificate), KeyVaultJsonContext.Default.CertificateDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating certificate {CertificateName} in vault {VaultName}", options.Certificate, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
