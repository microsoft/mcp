// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Azure.Mcp.Core.Commands;

public abstract class GlobalCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : BaseCommand
    where TOptions : GlobalOptions, new()
{
    protected readonly Option<string> _tenantOption = OptionDefinitions.Common.Tenant;
    protected readonly Option<AuthMethod> _authMethodOption = OptionDefinitions.Common.AuthMethod;
    protected readonly Option<string> _resourceGroupOption = OptionDefinitions.Common.ResourceGroup;
    protected readonly Option<int> _retryMaxRetries = OptionDefinitions.RetryPolicy.MaxRetries;
    protected readonly Option<double> _retryDelayOption = OptionDefinitions.RetryPolicy.Delay;
    protected readonly Option<double> _retryMaxDelayOption = OptionDefinitions.RetryPolicy.MaxDelay;
    protected readonly Option<RetryMode> _retryModeOption = OptionDefinitions.RetryPolicy.Mode;
    protected readonly Option<double> _retryNetworkTimeoutOption = OptionDefinitions.RetryPolicy.NetworkTimeout;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add global options
        command.AddOption(_tenantOption);
        command.AddOption(_authMethodOption);
        command.AddOption(_retryDelayOption);
        command.AddOption(_retryMaxDelayOption);
        command.AddOption(_retryMaxRetries);
        command.AddOption(_retryModeOption);
        command.AddOption(_retryNetworkTimeoutOption);
    }

    // Helper to get the command path for examples
    protected virtual string GetCommandPath()
    {
        // Get the command type name without the "Command" suffix
        string commandName = GetType().Name.Replace("Command", "");

        // Get the namespace to determine the service name
        string namespaceName = GetType().Namespace ?? "";
        string serviceName = "";

        // Extract service name from namespace (e.g., Azure.Mcp.Tools.Cosmos.Commands -> cosmos)
        if (!string.IsNullOrEmpty(namespaceName) && namespaceName.Contains(".Commands."))
        {
            string[] parts = namespaceName.Split(".Commands.");
            if (parts.Length > 1)
            {
                string[] subParts = parts[1].Split('.');
                if (subParts.Length > 0)
                {
                    serviceName = subParts[0].ToLowerInvariant();
                }
            }
        }

        // Insert spaces before capital letters in the command name
        string formattedName = string.Concat(commandName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim();

        // Convert to lowercase and replace spaces with spaces (for readability in command examples)
        string commandPath = formattedName.ToLowerInvariant().Replace(" ", " ");

        // Prepend the service name if available
        if (!string.IsNullOrEmpty(serviceName))
        {
            commandPath = serviceName + " " + commandPath;
        }

        return commandPath;
    }
    protected virtual TOptions BindOptions(ParseResult parseResult)
    {
        var options = new TOptions
        {
            Tenant = parseResult.GetValueForOption(_tenantOption),
            AuthMethod = parseResult.GetValueForOption(_authMethodOption)
        };

        if (UsesResourceGroup)
        {
            options.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        }

        // Only create RetryPolicy if any retry options are specified
        if (parseResult.HasAnyRetryOptions())
        {
            options.RetryPolicy = new RetryPolicyOptions
            {
                MaxRetries = parseResult.GetValueForOption(_retryMaxRetries),
                DelaySeconds = parseResult.GetValueForOption(_retryDelayOption),
                MaxDelaySeconds = parseResult.GetValueForOption(_retryMaxDelayOption),
                Mode = parseResult.GetValueForOption(_retryModeOption),
                NetworkTimeoutSeconds = parseResult.GetValueForOption(_retryNetworkTimeoutOption)
            };
        }

        return options;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        AuthenticationFailedException authEx =>
            $"Authentication failed. Please run 'az login' to sign in to Azure. Details: {authEx.Message}",
        RequestFailedException rfEx => rfEx.Message,
        HttpRequestException httpEx =>
            $"Service unavailable or network connectivity issues. Details: {httpEx.Message}",
        _ => ex.Message  // Just return the actual exception message
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        AuthenticationFailedException => 401,
        RequestFailedException rfEx => rfEx.Status,
        HttpRequestException => 503,
        _ => 500
    };
}
