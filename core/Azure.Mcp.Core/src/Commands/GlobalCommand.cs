// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
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
        command.Options.Add(_tenantOption);
        command.Options.Add(_authMethodOption);
        command.Options.Add(_retryDelayOption);
        command.Options.Add(_retryMaxDelayOption);
        command.Options.Add(_retryMaxRetries);
        command.Options.Add(_retryModeOption);
        command.Options.Add(_retryNetworkTimeoutOption);
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
            Tenant = parseResult.GetValue(_tenantOption),
            AuthMethod = parseResult.GetValue(_authMethodOption)
        };

        if (UsesResourceGroup)
        {
            options.ResourceGroup = parseResult.GetValue(_resourceGroupOption);
        }

        // Create a RetryPolicyOptions capturing only explicitly provided values so unspecified settings remain SDK defaults
        var hasAnyRetry = Azure.Mcp.Core.Options.ParseResultExtensions.HasAnyRetryOptions(parseResult);
        if (hasAnyRetry)
        {
            var policy = new RetryPolicyOptions();

            if (parseResult.GetResult(_retryMaxRetries) != null)
            {
                policy.HasMaxRetries = true;
                policy.MaxRetries = parseResult.GetValue(_retryMaxRetries);
            }
            if (parseResult.GetResult(_retryDelayOption) != null)
            {
                policy.HasDelaySeconds = true;
                policy.DelaySeconds = parseResult.GetValue(_retryDelayOption);
            }
            if (parseResult.GetResult(_retryMaxDelayOption) != null)
            {
                policy.HasMaxDelaySeconds = true;
                policy.MaxDelaySeconds = parseResult.GetValue(_retryMaxDelayOption);
            }
            if (parseResult.GetResult(_retryModeOption) != null)
            {
                policy.HasMode = true;
                policy.Mode = parseResult.GetValue(_retryModeOption);
            }
            if (parseResult.GetResult(_retryNetworkTimeoutOption) != null)
            {
                policy.HasNetworkTimeoutSeconds = true;
                policy.NetworkTimeoutSeconds = parseResult.GetValue(_retryNetworkTimeoutOption);
            }

            // Only assign if at least one flag set (defensive)
            if (policy.HasMaxRetries || policy.HasDelaySeconds || policy.HasMaxDelaySeconds || policy.HasMode || policy.HasNetworkTimeoutSeconds)
            {
                options.RetryPolicy = policy;
            }
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
