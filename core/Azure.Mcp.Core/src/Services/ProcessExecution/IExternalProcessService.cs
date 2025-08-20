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
    /// Executes an external process and returns the result
    /// </summary>
    /// <param name="executableName">Name of the executable to find in PATH or common install locations</param>
    /// <param name="arguments">Arguments to pass to the executable</param>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <param name="customPaths">Optional additional paths to search for the executable</param>
    /// <returns>Process execution result containing exit code, output and error streams</returns>
    Task<ProcessResult> ExecuteAsync(
        string executableName,
        string arguments,
        int timeoutSeconds = 300,
        IEnumerable<string>? customPaths = null);

    /// <summary>
    /// Tries to parse the process output as JSON and return it as JsonElement
    /// </summary>
    /// <param name="result">Process execution result</param>
    /// <returns>Parsed JSON element or formatted error object if parsing fails</returns>
    JsonElement ParseJsonOutput(ProcessResult result);

    /// <summary>
    /// Sets environment variables for the process execution
    /// /// </summary>
    /// <param name="variables">Dictionary of environment variables to set</param>
    void SetEnvironmentVariables(IDictionary<string, string> variables);
}
