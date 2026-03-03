// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Options;
using Azure.Mcp.Tools.BicepSchema.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.BicepSchema.Commands
{
    public sealed class BicepSchemaGetCommand(ILogger<BicepSchemaGetCommand> logger, IBicepSchemaService bicepSchemaService) : BaseBicepSchemaCommand<BicepSchemaOptions>
    {
        private const string CommandTitle = "Get Bicep Schema for a resource";

        private readonly ILogger<BicepSchemaGetCommand> _logger = logger;
        private readonly IBicepSchemaService _bicepSchemaService = bicepSchemaService;

        public override string Id => "553c003a-7cdf-4382-b833-94fe8bbb7386";

        public override string Name => "get";

        public override string Description =>
       """
        Provides the Bicep schema definition of any Azure resource type (latest service version). Use this to get the schema needed to write Bicep IaC (infrasturcture as code) for Azure resources such as AI models, storage accounts, databases, virtual machines, app services, key vaults, and more. Do not use this tool for resource deployment, deployment guidelines, or getting best practices.
       """;

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

        public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return Task.FromResult(context.Response);
            }

            BicepSchemaOptions options = BindOptions(parseResult);

            try
            {
                TypesDefinitionResult result = _bicepSchemaService.GetResourceTypeDefinitions(options.ResourceType!);
                List<ComplexType> response = SchemaGenerator.GetResponse(result);

                if (response is not null)
                {
                    // Only log the resource type if we are able to get the schema from it.
                    // There is a slight chance that the LLM hallucinates the resource type
                    // parameter with value containing data that we shouldn't log.
                    context.Activity?.AddTag("resourceType", options.ResourceType);
                    context.Response.Results = ResponseResult.Create(new(response),
                        BicepSchemaJsonContext.Default.BicepSchemaGetCommandResult);
                }
                else
                {
                    context.Response.Results = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred fetching Bicep schema.");
                HandleException(context, ex);
            }
            return Task.FromResult(context.Response);

        }

        internal record BicepSchemaGetCommandResult(List<ComplexType> BicepSchemaResult);
    }
}
