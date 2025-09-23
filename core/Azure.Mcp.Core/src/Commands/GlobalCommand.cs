// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Azure.Mcp.Core.Commands;

public abstract class GlobalCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : BaseCommand<TOptions>
    where TOptions : GlobalOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add global options
        command.Options.Add(OptionDefinitions.Common.Tenant);
        command.Options.Add(OptionDefinitions.Common.AuthMethod);
        command.Options.Add(OptionDefinitions.RetryPolicy.Delay);
        command.Options.Add(OptionDefinitions.RetryPolicy.MaxDelay);
        command.Options.Add(OptionDefinitions.RetryPolicy.MaxRetries);
        command.Options.Add(OptionDefinitions.RetryPolicy.Mode);
        command.Options.Add(OptionDefinitions.RetryPolicy.NetworkTimeout);
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
    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = new TOptions
        {
            Tenant = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Tenant.Name),
            AuthMethod = parseResult.GetValueOrDefault<AuthMethod>(OptionDefinitions.Common.AuthMethod.Name)
        };

        // Create a RetryPolicyOptions capturing only explicitly provided values so unspecified settings remain SDK defaults
        var hasAnyRetry = Options.ParseResultExtensions.HasAnyRetryOptions(parseResult);
        if (hasAnyRetry)
        {
            var policy = new RetryPolicyOptions();

            if (parseResult.GetResult(OptionDefinitions.RetryPolicy.MaxRetries) != null)
            {
                policy.HasMaxRetries = true;
                policy.MaxRetries = parseResult.GetValueOrDefault<int>(OptionDefinitions.RetryPolicy.MaxRetries.Name);
            }
            if (parseResult.GetResult(OptionDefinitions.RetryPolicy.Delay) != null)
            {
                policy.HasDelaySeconds = true;
                policy.DelaySeconds = parseResult.GetValueOrDefault<double>(OptionDefinitions.RetryPolicy.Delay.Name);
            }
            if (parseResult.GetResult(OptionDefinitions.RetryPolicy.MaxDelay) != null)
            {
                policy.HasMaxDelaySeconds = true;
                policy.MaxDelaySeconds = parseResult.GetValueOrDefault<double>(OptionDefinitions.RetryPolicy.MaxDelay.Name);
            }
            if (parseResult.GetResult(OptionDefinitions.RetryPolicy.Mode) != null)
            {
                policy.HasMode = true;
                policy.Mode = parseResult.GetValueOrDefault<RetryMode>(OptionDefinitions.RetryPolicy.Mode.Name);
            }
            if (parseResult.GetResult(OptionDefinitions.RetryPolicy.NetworkTimeout) != null)
            {
                policy.HasNetworkTimeoutSeconds = true;
                policy.NetworkTimeoutSeconds = parseResult.GetValueOrDefault<double>(OptionDefinitions.RetryPolicy.NetworkTimeout.Name);
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        RequestFailedException rfEx => (HttpStatusCode)rfEx.Status,
        HttpRequestException => HttpStatusCode.ServiceUnavailable,
        _ => HttpStatusCode.InternalServerError
    };
}
