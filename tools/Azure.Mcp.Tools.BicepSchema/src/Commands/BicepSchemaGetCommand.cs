// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.BicepSchema.Options;
using Azure.Mcp.Tools.BicepSchema.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.BicepSchema.Commands
{
    public sealed class BicepSchemaGetCommand(ILogger<BicepSchemaGetCommand> logger) : BaseBicepSchemaCommand<BicepSchemaOptions>
    {
        private const string CommandTitle = "Get Bicep Schema for a resource";

        private readonly ILogger<BicepSchemaGetCommand> _logger = logger;
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

        private static readonly Lazy<IServiceProvider> s_serviceProvider;

        static BicepSchemaGetCommand()
        {
            s_serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                var serviceCollection = new ServiceCollection();
                SchemaGenerator.ConfigureServices(serviceCollection);
                return serviceCollection.BuildServiceProvider();
            });
        }

        public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return Task.FromResult(context.Response);
            }

            BicepSchemaOptions options = BindOptions(parseResult);

            try
            {
                TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(s_serviceProvider.Value, options.ResourceType!);
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
