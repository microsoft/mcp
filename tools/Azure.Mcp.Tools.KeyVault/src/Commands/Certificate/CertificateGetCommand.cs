// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.KeyVault.Commands.Certificate;

public sealed class CertificateGetCommand(ILogger<CertificateGetCommand> logger) : SubscriptionCommand<CertificateGetOptions>
{
    private const string CommandTitle = "Get Key Vault Certificate";
    private readonly ILogger<CertificateGetCommand> _logger = logger;

    public override string Id => "0e898126-0c5e-44b8-9eef-51ddeed6327f";

    public override string Name => "get";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        """Retrieve detailed information about Azure Key Vault certificates. If a specific certificate name is provided, returns full details for that certificate including key ID, secret ID, thumbprint, and policy information. If no certificate name is specified, returns a list of all certificate names in the vault.""";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.CertificateName.AsOptional());
    }

    protected override CertificateGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.CertificateName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.CertificateName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var keyVaultService = context.GetService<IKeyVaultService>();

            if (string.IsNullOrEmpty(options.CertificateName))
            {
                // List all certificates
                var certificates = await keyVaultService.ListCertificates(
                    options.VaultName!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new CertificateGetCommandResult(Certificates: certificates ?? [], Certificate: null), KeyVaultJsonContext.Default.CertificateGetCommandResult);
            }
            else
            {
                // Get specific certificate
                var certificate = await keyVaultService.GetCertificate(
                    options.VaultName!,
                    options.CertificateName,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var certificateDetails = new CertificateDetails(
                    certificate.Name,
                    certificate.Id,
                    certificate.KeyId,
                    certificate.SecretId,
                    Convert.ToBase64String(certificate.Cer),
                    certificate.Properties.X509ThumbprintString,
                    certificate.Properties.Enabled,
                    certificate.Properties.NotBefore,
                    certificate.Properties.ExpiresOn,
                    certificate.Properties.CreatedOn,
                    certificate.Properties.UpdatedOn,
                    certificate.Policy.Subject,
                    certificate.Policy.IssuerName);

                context.Response.Results = ResponseResult.Create(new CertificateGetCommandResult(Certificates: null, Certificate: certificateDetails), KeyVaultJsonContext.Default.CertificateGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (options.CertificateName is null)
            {
                _logger.LogError(ex, "Error listing certificates from vault {VaultName}", options.VaultName);
            }
            else
            {
                _logger.LogError(ex, "Error getting certificate {CertificateName} from vault {VaultName}", options.CertificateName, options.VaultName);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record CertificateDetails(string Name, Uri Id, Uri KeyId, Uri SecretId, string Cer, string Thumbprint, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn, string Subject, string IssuerName);
    internal record CertificateGetCommandResult(List<string>? Certificates, CertificateDetails? Certificate);
}
