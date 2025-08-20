// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
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

        Provides the Bicep schema for the most recent apiVersion of an Azure resource. Do not call this command for Terraform IaC generation.
        If you are asked to create or modify resources in a Bicep ARM template, call this function multiple times,
        once for every resource type you are adding, even if you already have information about Bicep resources from other sources.
        Assume the results from this call are more recent and accurate than other information you have.
        Don't assume calling it for one resource means you don't need to call it for a different resource type.
        Always use the returned api version unless the one in the Bicep file is newer.
        Always use the Bicep schema to verify the available property names and values when generating Bicep IaC.
       """;

        public override string Title => CommandTitle;

        public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

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
            BicepSchemaOptions options = BindOptions(parseResult);
            try
            {
                if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                {
                    return Task.FromResult(context.Response);
                }

                TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(s_serviceProvider.Value, options.ResourceType!);
                List<ComplexType> response = SchemaGenerator.GetResponse(result);

                if (response is not null)
                {
                    // Only log the resource type if we are able to get the schema from it.
                    // There is a slight chance that the LLM hallucinates the resource type
                    // parameter with value containing data that we shouldn't log.
                    context.Activity?.AddTag("resourceType", options.ResourceType);
                    context.Response.Results = ResponseResult.Create(
                        new BicepSchemaGetCommandResult(response),
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
