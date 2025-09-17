// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Certificate;

public sealed class CertificateImportCommand(ILogger<CertificateImportCommand> logger) : SubscriptionCommand<CertificateImportOptions>
{
    private const string CommandTitle = "Import Key Vault Certificate";
    private readonly ILogger<CertificateImportCommand> _logger = logger;

    public override string Name => "import";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    public override string Description =>
        """
        Imports (uploads) an existing certificate (PFX or PEM with private key) into an Azure Key Vault without generating
        a new certificate or key material. This command accepts either a file path to a PFX/PEM file, a base64 encoded PFX,
        or raw PEM text starting with -----BEGIN. If the certificate is a password-protected PFX, a password must be provided.
        Returns certificate details including name, id, keyId, secretId, cer (base64), thumbprint, validity, and policy
        subject/issuer.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KeyVaultOptionDefinitions.VaultName);
        command.Options.Add(KeyVaultOptionDefinitions.CertificateName);
        command.Options.Add(KeyVaultOptionDefinitions.CertificateData);
        command.Options.Add(KeyVaultOptionDefinitions.CertificatePassword);
    }

    protected override CertificateImportOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.VaultName.Name);
        options.CertificateName = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.CertificateName.Name);
        options.CertificateData = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.CertificateData.Name);
        options.Password = parseResult.GetValueOrDefault<string>(KeyVaultOptionDefinitions.CertificatePassword.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var keyVaultService = context.GetService<IKeyVaultService>();

            var certificate = await keyVaultService.ImportCertificate(
                options.VaultName!,
                options.CertificateName!,
                options.CertificateData!,
                options.Password,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new CertificateImportCommandResult(
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
                    certificate.Policy.IssuerName),
                KeyVaultJsonContext.Default.CertificateImportCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing certificate {CertificateName} into vault {VaultName}", options.CertificateName, options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record CertificateImportCommandResult(string Name, Uri Id, Uri KeyId, Uri SecretId, string Cer, string Thumbprint, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn, string Subject, string IssuerName);
}
