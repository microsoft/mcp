// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Query;
using Azure.Mcp.Tools.IoTHub.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Query;

[CommandMetadata(
    Id = "e20e55ae-6c05-4442-94e9-c0a53088504f",
    Name = "compile",
    Title = "Compile IoT Hub Query",
    Description = """
        Compile a structured set of typed predicates into a syntactically valid IoT Hub query string.
        Instead of writing raw IoT Hub SQL, supply a JSON array of predicates via --filters where each predicate specifies a 'scope' (device, tags, desired, or reported), a 'field' (the property name/path within that scope), an 'operator' (equals, notEquals, lessThan, lessThanOrEqual, greaterThan, greaterThanOrEqual), and a 'value' (string, number, or boolean).
        The compiler maps each predicate to the correct field path (tags.*, properties.desired.*, properties.reported.*, or a top-level device field), validates it, and joins the predicates with --logical-operator (AND by default). Pass the 'fields' object returned by 'iothub query discover' to --discovered-fields to reject filters that reference paths not found in sampled device twins. Optionally set --top to return a 'maxCount' hint for 'iothub query run', and --from to target 'devices' (default), 'devices.modules', or 'devices.jobs'.
        Returns an object with a single 'query' string that can be passed directly to 'iothub query run' along with the returned maxCount. This command performs no network calls.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubQueryCompileCommand(
    ILogger<IoTHubQueryCompileCommand> logger)
    : BaseCommand<IoTHubQueryCompileOptions, IoTHubQueryCompileResult>
{
    private readonly ILogger<IoTHubQueryCompileCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubQueryCompileOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            List<QueryPredicate>? filters;
            try
            {
                filters = JsonSerializer.Deserialize(options.Filters, IoTHubJsonContext.Default.ListQueryPredicate);
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

            // Cap the returned page-size hint to the maximum that 'iothub query run' will honor so callers
            // are not misled into expecting a larger page than a single run can return.
            var maxCount = options.Top is { } top ? Math.Min(top, IoTHubQueryLimits.MaxPageSize) : (int?)null;
            var result = new IoTHubQueryCompileResult(query, maxCount);
            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.IoTHubQueryCompileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling IoT Hub query.");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
