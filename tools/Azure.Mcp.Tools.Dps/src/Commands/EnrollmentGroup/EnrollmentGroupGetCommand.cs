using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Dps.Options;
using Azure.Mcp.Tools.Dps.Options.EnrollmentGroup;
using Azure.Mcp.Tools.Dps.Services;

namespace Azure.Mcp.Tools.Dps.Commands.EnrollmentGroup
{
    internal class EnrollmentGroupGetCommand : GlobalCommand<EnrollmentGroupOptions>
    {
        public override string Id => "c0159631-d9f3-4283-99b6-9eac5a73ee06";

        public override string Name => "get";

        public override string Description => "Get enrollment group";

        public override string Title => "Get enrollment group";

        public override ToolMetadata Metadata => new()
        {
            Destructive = false,
            Idempotent = true,
            OpenWorld = false,
            ReadOnly = true,
            LocalRequired = false,
            Secret = false,
        };

        protected override void RegisterOptions(Command command)
        {
            base.RegisterOptions(command);
            command.Options.Add(DpsOptionDefinitions.Hostname);
            command.Options.Add(DpsOptionDefinitions.EnrollmentGroupId);
        }

        protected override EnrollmentGroupOptions BindOptions(ParseResult parseResult)
        {
            EnrollmentGroupOptions opts = base.BindOptions(parseResult);
            opts.DpsHostName = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.Hostname.Name) ?? string.Empty;
            opts.EnrollmentGroupId = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.EnrollmentGroupId.Name) ?? string.Empty;
            return opts;
        }

        public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
        {
            EnrollmentGroupOptions options = BindOptions(parseResult);

            IEnrollmentGroupService enrollmentGroupService = context.GetService<IEnrollmentGroupService>();

            Models.EnrollmentGroup result = await enrollmentGroupService.GetEnrollmentGroupAsync(options.DpsHostName, options.EnrollmentGroupId, cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new EGGetCommandResult(result),
                EgGetContext.Default.EGGetCommandResult
            );

            return context.Response;
        }

        internal record EGGetCommandResult([property: JsonPropertyName("enrollmentGroup")] Models.EnrollmentGroup enrollmentGroup);
    }

    [JsonSerializable(typeof(EnrollmentGroupGetCommand.EGGetCommandResult))]
    [JsonSerializable(typeof(Models.EnrollmentGroup))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class EgGetContext : JsonSerializerContext;
}
