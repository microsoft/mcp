// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Tools.Policy.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Services;

public class PolicyService(IExternalProcessService processService, ILogger<PolicyService> logger) : IPolicyService
{
    private readonly IExternalProcessService _processService = processService;
    private readonly ILogger<PolicyService> _logger = logger;

    public async Task<List<PolicyAssignment>> GetAssignmentsAsync(
        string? subscription = null,
        string? assignment = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build the Azure CLI command
            var commandArgs = new List<string> { "policy", "assignment", "list" };

            if (!string.IsNullOrEmpty(subscription))
            {
                commandArgs.Add("--subscription");
                commandArgs.Add(subscription);
            }

            if (!string.IsNullOrEmpty(assignment))
            {
                // When a specific assignment is requested, use 'show' instead of 'list'
                commandArgs[2] = "show";
                commandArgs.Add("--name");
                commandArgs.Add(assignment);
            }

            commandArgs.Add("--output");
            commandArgs.Add("json");

            var command = string.Join(" ", commandArgs);
            _logger.LogDebug("Executing Azure CLI command: az {Command}", command);

            var result = await _processService.ExecuteAsync("az", command, timeoutSeconds: 60);

            if (result.ExitCode != 0)
            {
                _logger.LogError("Azure CLI command failed with exit code {ExitCode}. Error: {Error}", result.ExitCode, result.Error);
                throw new InvalidOperationException($"Azure CLI command failed: {result.Error}");
            }

            var jsonElement = _processService.ParseJsonOutput(result);

            // Handle both single assignment (show) and list of assignments
            List<PolicyAssignment> assignments;
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                assignments = JsonSerializer.Deserialize<List<PolicyAssignment>>(jsonElement.GetRawText()) ?? [];
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var singleAssignment = JsonSerializer.Deserialize<PolicyAssignment>(jsonElement.GetRawText());
                assignments = singleAssignment != null ? [singleAssignment] : [];
            }
            else
            {
                _logger.LogWarning("Unexpected JSON format from Azure CLI");
                assignments = [];
            }

            return assignments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving policy assignments");
            throw;
        }
    }
}
