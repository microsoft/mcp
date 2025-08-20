// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.KeyVault.Options;
using Azure.Mcp.Tools.KeyVault.Options.Certificate;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.KeyVault.Commands.Certificate;

public sealed class CertificateListCommand(ILogger<CertificateListCommand> logger) : SubscriptionCommand<CertificateListOptions>
{
    private const string _commandTitle = "List Key Vault Certificates";
    private readonly ILogger<CertificateListCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;

    public override string Name => "list";

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override string Description =>
        """
        List all certificates in an Azure Key Vault. This command retrieves and displays the names of all certificates
        stored in the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
    }

    protected override CertificateListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueForOption(_vaultOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var keyVaultService = context.GetService<IKeyVaultService>();
            var certificates = await keyVaultService.ListCertificates(
                options.VaultName!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = certificates?.Count > 0 ?
                ResponseResult.Create(
                    new CertificateListCommandResult(certificates),
                    KeyVaultJsonContext.Default.CertificateListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing certificates from vault {VaultName}.", options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record CertificateListCommandResult(List<string> Certificates);
}
