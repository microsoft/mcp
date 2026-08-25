// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Mcp.Core.Options;
using TrimmedOptionContainerApp;

var command = new Command("test");
OptionBinder.RegisterOptions<TrimmedOptions>(command);

string[] expectedOptions =
[
    "--retry-delay",
    "--retry-max-delay",
    "--retry-max-retries",
    "--retry-mode",
    "--retry-network-timeout"
];
var missingOptions = expectedOptions.Except(command.Options.Select(option => option.Name)).ToArray();

if (missingOptions.Length > 0)
{
    Console.Error.WriteLine($"Missing nested options: {string.Join(", ", missingOptions)}");
    return 1;
}

var parseResult = command.Parse("--retry-max-retries 7");
var options = OptionBinder.BindOptions<TrimmedOptions>(parseResult);
if (options.RetryPolicy?.MaxRetries != 7)
{
    Console.Error.WriteLine("Failed to bind --retry-max-retries from the trimmed executable.");
    return 1;
}

return 0;
