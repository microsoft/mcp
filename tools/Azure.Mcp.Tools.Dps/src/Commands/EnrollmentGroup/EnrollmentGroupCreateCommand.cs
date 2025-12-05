using System.CommandLine;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Dps.Options;
using Azure.Mcp.Tools.Dps.Options.EnrollmentGroup;
using Azure.Mcp.Tools.Dps.Services;

namespace Azure.Mcp.Tools.Dps.Commands.EnrollmentGroup;

/// <summary>
/// Command to create an enrollment group.
/// </summary>
public sealed class EnrollmentGroupCreateCommand : GlobalCommand<EnrollmentGroupCreateOptions>
{
    public override string Id => "e3e1b9e5-2c1a-4c6e-9e2d-4b7e8c1c2d3f";
    public override string Name => "create";
    public override string Description => "Create a new enrollment group";
    public override string Title => "Create enrollment group";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false,
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        var hostnameOpt = new Option<string>($"--{DpsOptionDefinitions.HostnameName}") { Description = DpsOptionDefinitions.Hostname.Description };
        command.Options.Add(hostnameOpt);

        var groupIdOpt = new Option<string>($"--{DpsOptionDefinitions.EnrollmentGroupIdName}") { Description = DpsOptionDefinitions.EnrollmentGroupId.Description };
        command.Options.Add(groupIdOpt);

        var attestationTypeOpt = new Option<string>("--attestationType") { Description = "Attestation type (symmetricKey or x509)" };
        command.Options.Add(attestationTypeOpt);

        var primaryKeyOpt = new Option<string>("--primaryKey") { Description = "Primary key for symmetric attestation" };
        command.Options.Add(primaryKeyOpt);

        var secondaryKeyOpt = new Option<string>("--secondaryKey") { Description = "Secondary key for symmetric attestation" };
        command.Options.Add(secondaryKeyOpt);

        var certificateOpt = new Option<string>("--certificate") { Description = "Certificate for x509 attestation" };
        command.Options.Add(certificateOpt);

        var provisioningStatusOpt = new Option<string>("--provisioningStatus") { Description = "Provisioning status (enabled or disabled)" };
        command.Options.Add(provisioningStatusOpt);
    }

    protected override EnrollmentGroupCreateOptions BindOptions(ParseResult parseResult)
    {
        EnrollmentGroupCreateOptions opts = base.BindOptions(parseResult);
        opts.DpsHostName = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.Hostname.Name) ?? string.Empty;
        opts.EnrollmentGroupId = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.EnrollmentGroupId.Name) ?? string.Empty;
        opts.AttestationType = parseResult.GetValueOrDefault<string>("attestationType") ?? string.Empty;
        opts.PrimaryKey = parseResult.GetValueOrDefault<string>("primaryKey");
        opts.SecondaryKey = parseResult.GetValueOrDefault<string>("secondaryKey");
        opts.Certificate = parseResult.GetValueOrDefault<string>("certificate");
        opts.ProvisioningStatus = parseResult.GetValueOrDefault<string>("provisioningStatus");
        return opts;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        IEnrollmentGroupService enrollmentGroupService = context.GetService<IEnrollmentGroupService>();

        try
        {
            var attestation = new Models.Attestation
            {
                SymmetricKey = new Models.SymmetricKeyAttestation
                {
                    PrimaryKey = string.IsNullOrEmpty(options.PrimaryKey) ?
                    /* new base 64 string*/
                    Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    : options.PrimaryKey,
                    SecondaryKey = string.IsNullOrEmpty(options.SecondaryKey) ?
                    /* new base 64 string*/
                    Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    : options.SecondaryKey,
                },
            };

            var enrollmentGroup = new Models.EnrollmentGroup
            {
                EnrollmentGroupId = options.EnrollmentGroupId,
                ProvisioningStatus = options.ProvisioningStatus ?? string.Empty,
                Attestation = attestation
            };

            var result = await enrollmentGroupService.CreateOrUpdateAsync(
                options.DpsHostName,
                enrollmentGroup,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new EGCreateCommandResult(result),
                EgCreateContext.Default.EGCreateCommandResult
            );
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record EGCreateCommandResult([property: JsonPropertyName("enrollmentGroup")] Models.EnrollmentGroup EnrollmentGroup);
}

[JsonSerializable(typeof(EnrollmentGroupCreateCommand.EGCreateCommandResult))]
[JsonSerializable(typeof(Models.EnrollmentGroup))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class EgCreateContext : JsonSerializerContext;
