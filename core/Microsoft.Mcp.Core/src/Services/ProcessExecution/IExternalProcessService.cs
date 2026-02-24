// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.ProcessExecution;

public record ProcessResult(
    int ExitCode,
    string Output,
    string Error,
    string Command);

public interface IExternalProcessService
{
    /// <summary>
    /// Executes an external process and captures its standard output and error streams.
    /// </summary>
    /// <param name="executablePath">Full path to the executable to run.</param>
    /// <param name="arguments">Command-line arguments to pass to the executable.</param>
    /// <param name="environmentVariables">
    /// Optional environment variables to set for the process. If null or not provided, no additional
    /// environment variables will be set beyond those inherited from the parent process.
    /// </param>
    /// <param name="operationTimeoutSeconds">
    /// Total timeout in seconds for the entire operation, including process execution and output capture.
    /// Defaults to 300 seconds (5 minutes). Must be greater than zero.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to abort the operation. The process will be terminated if cancellation is requested.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessResult"/> containing the exit code, standard output, standard error, and command string.
    /// </returns>
    Task<ProcessResult> ExecuteAsync(
        string executablePath,
        string arguments,
        IDictionary<string, string>? environmentVariables = default,
        int operationTimeoutSeconds = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to parse the process output as JSON and return it as JsonElement
    /// </summary>
    /// <param name="result">Process execution result</param>
    /// <returns>Parsed JSON element or formatted error object if parsing fails</returns>
    JsonElement ParseJsonOutput(ProcessResult result);
}
