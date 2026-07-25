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
    Id = "0e898126-0c5e-44b8-9eef-51ddeed6327f",
    Name = "get",
    Title = "Get Key Vault Certificate",
    Description = "List all certificates in your Key Vault or get a specific certificate by name. Shows all certificate names in the vault, or retrieves full certificate details including key ID, secret ID, thumbprint, and policy information.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CertificateGetCommand(ILogger<CertificateGetCommand> logger, IKeyVaultService keyVaultService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CertificateGetOptions, CertificateGetCommand.CertificateGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CertificateGetCommand> _logger = logger;
    private readonly IKeyVaultService _keyVaultService = keyVaultService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CertificateGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(options.Certificate))
            {
                // List all certificates
                var certificates = await _keyVaultService.ListCertificates(
                    options.Vault,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(certificates ?? [], null), KeyVaultJsonContext.Default.CertificateGetCommandResult);
            }
            else
            {
                // Get specific certificate
                var certificate = await _keyVaultService.GetCertificate(
                    options.Vault,
                    options.Certificate,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, CertificateDetails.FromCertificate(certificate)), KeyVaultJsonContext.Default.CertificateGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.Certificate))
            {
                _logger.LogError(ex, "Error listing certificates from vault {VaultName}", options.Vault);
            }
            else
            {
                _logger.LogError(ex, "Error getting certificate {CertificateName} from vault {VaultName}", options.Certificate, options.Vault);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CertificateGetCommandResult(List<string>? Certificates, CertificateDetails? Certificate);
}
