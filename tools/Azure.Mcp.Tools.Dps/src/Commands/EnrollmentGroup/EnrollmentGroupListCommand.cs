using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Dps.Options;
using Azure.Mcp.Tools.Dps.Options.EnrollmentGroup;
using Azure.Mcp.Tools.Dps.Services;

namespace Azure.Mcp.Tools.Dps.Commands.EnrollmentGroup
{
    internal class EnrollmentGroupListCommand : GlobalCommand<EnrollmentGroupOptions>
    {
        public override string Id => "c0159631-d9f3-4183-99b6-9eac5a73ee06";

        public override string Name => "list";

        public override string Description => "List names of all enrollment groups";

        public override string Title => "List names of all enrollment groups";

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
        }

        protected override EnrollmentGroupOptions BindOptions(ParseResult parseResult)
        {
            EnrollmentGroupOptions opts = base.BindOptions(parseResult);
            opts.DpsHostName = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.Hostname.Name) ?? string.Empty;
            return opts;
        }

        public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
        {
            EnrollmentGroupOptions options = BindOptions(parseResult);

            IEnrollmentGroupService enrollmentGroupService = context.GetService<IEnrollmentGroupService>();

            IList<Models.EnrollmentGroup> result = await enrollmentGroupService.ListAllEnrollmentGroupsAsync(options.DpsHostName, cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new EGListCommandResult(
                    [.. result]
                ),
                EgContext.Default.EGListCommandResult
            );

            return context.Response;
        }

        internal record EGListCommandResult([property: JsonPropertyName("enrollmentGroups")] List<Models.EnrollmentGroup> enrollmentGroups);
    }

    [JsonSerializable(typeof(EnrollmentGroupListCommand.EGListCommandResult))]
    [JsonSerializable(typeof(Models.EnrollmentGroup))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class EgContext : JsonSerializerContext;
}
