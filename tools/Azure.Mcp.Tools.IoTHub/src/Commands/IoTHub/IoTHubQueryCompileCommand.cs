// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubQueryCompileCommand(ILogger<IoTHubQueryCompileCommand> logger)
    : BaseCommand<IoTHubQueryCompileOptions>
{
    public override string Id => "iothub-query-compile";
    public override string Name => "compile";
    public override string Description => "Compile a structured set of typed predicates into a syntactically valid IoT Hub query string. " +
    "Instead of writing raw IoT Hub SQL, supply a JSON array of predicates via --filters where each predicate specifies a 'scope' " +
    "(device, tags, desired, or reported), a 'field' (the property name/path within that scope), an 'operator' " +
    "(equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and a 'value' (string, number, or boolean). " +
    "The compiler maps each predicate to the correct field path (tags.*, properties.desired.*, properties.reported.*, or a top-level device field), " +
    "validates it, and joins the predicates with --logical-operator (AND by default). Pass the 'fields' object returned by 'iothub query discover' " +
    "to --discovered-fields to reject filters that reference paths not found in sampled device twins. Optionally set --top to return a 'maxCount' hint for " +
    "target 'devices' (default), 'devices.modules', or 'devices.jobs'. Returns an object with a single 'query' string that can be passed directly " +
    "to 'iothub query run' along with the returned maxCount. Use this to build safe, unambiguous IoT Hub queries without hand-writing SQL. This command performs no network calls.";
    public override string Title => "Compile IoT Hub Query";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private readonly ILogger<IoTHubQueryCompileCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubQueryCompileOptionDefinitions.Filters.AsRequired());
        command.Options.Add(IoTHubQueryCompileOptionDefinitions.From.AsOptional());
        command.Options.Add(IoTHubQueryCompileOptionDefinitions.Top.AsOptional());
        command.Options.Add(IoTHubQueryCompileOptionDefinitions.LogicalOperator.AsOptional());
        command.Options.Add(IoTHubQueryCompileOptionDefinitions.DiscoveredFields.AsOptional());
    }

    protected override IoTHubQueryCompileOptions BindOptions(ParseResult parseResult)
    {
        var options = new IoTHubQueryCompileOptions
        {
            Filters = parseResult.GetValueOrDefault<string>(IoTHubQueryCompileOptionDefinitions.Filters.Name),
            From = parseResult.GetValueOrDefault<string>(IoTHubQueryCompileOptionDefinitions.From.Name),
            Top = parseResult.GetValueOrDefault<int?>(IoTHubQueryCompileOptionDefinitions.Top.Name),
            LogicalOperator = parseResult.GetValueOrDefault<string>(IoTHubQueryCompileOptionDefinitions.LogicalOperator.Name),
            DiscoveredFields = parseResult.GetValueOrDefault<string>(IoTHubQueryCompileOptionDefinitions.DiscoveredFields.Name)
        };
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            List<QueryPredicate>? filters;
            try
            {
                filters = JsonSerializer.Deserialize(options.Filters!, IoTHubJsonContext.Default.ListQueryPredicate);
            }
            catch (JsonException ex)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = $"The --filters value is not valid JSON: {ex.Message}";
                return Task.FromResult(context.Response);
            }

            if (filters is null || filters.Count == 0)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "The --filters value must be a non-empty JSON array of predicate objects.";
                return Task.FromResult(context.Response);
            }

            QueryDiscoveredFields? discoveredFields = null;
            if (!string.IsNullOrWhiteSpace(options.DiscoveredFields))
            {
                try
                {
                    discoveredFields = JsonSerializer.Deserialize(options.DiscoveredFields, IoTHubJsonContext.Default.QueryDiscoveredFields);
                }
                catch (JsonException ex)
                {
                    context.Response.Status = HttpStatusCode.BadRequest;
                    context.Response.Message = $"The --discovered-fields value is not valid JSON: {ex.Message}";
                    return Task.FromResult(context.Response);
                }
            }

            var request = new QueryCompileRequest
            {
                Filters = filters,
                From = string.IsNullOrWhiteSpace(options.From) ? "devices" : options.From!,
                Top = options.Top,
                LogicalOperator = string.IsNullOrWhiteSpace(options.LogicalOperator) ? "AND" : options.LogicalOperator!,
                DiscoveredFields = discoveredFields
            };

            string query;
            try
            {
                query = IoTHubQueryCompiler.Compile(request);
            }
            catch (ArgumentException ex)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = ex.Message;
                return Task.FromResult(context.Response);
            }

            var result = new IoTHubQueryCompileResult(query, options.Top);
            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.IoTHubQueryCompileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling IoT Hub query");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
