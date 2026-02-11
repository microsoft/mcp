// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using static Azure.Mcp.Core.Services.ProcessExecution.ProcessExtensions;

namespace Azure.Mcp.Core.Services.ProcessExecution;

/// <summary>
/// Executes external processes and captures their output in a timeout and cancellation aware way.
/// </summary>
public class ExternalProcessService(ILogger<ExternalProcessService> logger) : IExternalProcessService
{
    /// <summary>
    /// Executes an external process and captures its stdout and stderr.
    /// </summary>
    /// <param name="fileName">The name or path of the executable to run. In the case of a bare executable name, 
    /// the operating system resolves the executable using its standard search rules (including directories
    /// listed in the PATH environment variable).</param>
    /// <param name="arguments">Command-line arguments to pass to the process.</param>
    /// <param name="environmentVariables">
    /// Optional environment variables. If null, the process inherits the parent environment.
    /// </param>
    /// <param name="operationTimeoutSeconds">
    /// Total timeout for the operation (process execution + exit-wait + stdout/stderr drain).
    /// Must be greater than zero.
    /// </param>
    /// <param name="cancellationToken">
    /// External cancellation token. When triggered, the process is terminated on a best-effort basis
    /// and <see cref="OperationCanceledException"/> is rethrown.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessResult"/> containing the exit code, stdout, stderr, and the full command.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="operationTimeoutSeconds"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="fileName"/> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the process fails to start.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when the operation (process execution and stream draining) exceeds <paramref name="operationTimeoutSeconds"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is triggered before the operation completes.
    /// </exception>
    /// <remarks>
    /// <para><b>Timeout vs. cancellation:</b></para>
    /// <para>
    /// Timeout and cancellation are handled independently:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <paramref name="operationTimeoutSeconds"/> is enforced by wrapping the combined
    ///       (exit-wait + stdout + stderr) task in <see cref="Task.WaitAsync(TimeSpan, CancellationToken)"/>.
    ///       A timeout always results in <see cref="TimeoutException"/>.
    ///       If the timeout occurs before the process has exited, a best-effort attempt is made to
    ///       terminate the process (and its tree) before throwing.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <paramref name="cancellationToken"/> represents external caller cancellation. When cancellation is
    ///       signaled, and the process has not yet exited, a best-effort attempt is made to terminate the process
    ///       (and its tree) before rethrowing the original <see cref="OperationCanceledException"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// This separation avoids the common ambiguity where timeouts are implemented by canceling the caller’s token.
    /// Here: <see cref="TimeoutException"/> always means timeout; <see cref="OperationCanceledException"/>
    /// always means external cancellation.
    /// </para>
    ///
    /// <para>
    /// <b>Diagnostic information on exceptions:</b><br/>
    /// When a timeout or external cancellation occurs, this method attaches structured diagnostic
    /// information to the thrown <see cref="TimeoutException"/> or
    /// <see cref="OperationCanceledException"/> via the exception’s <c>Data</c> dictionary. This
    /// information does not alter exception semantics or stack traces, and exists solely to aid
    /// debugging. The following fields are always included:
    /// </para>
    ///
    /// <list type="bullet">
    ///   <item><description><c>"ProcessName"</c> — the process name if available, or <c>"&lt;unknown&gt;"</c>.</description></item>
    ///   <item><description><c>"ProcessId"</c> — the process ID if available, or <c>-1</c>.</description></item>
    ///   <item><description><c>"ProcessExitStatus"</c> — one of <c>Exited</c>, <c>NotExited</c>, or <c>Indeterminate</c>,
    ///       indicating the observed exit state of the process "before" any kill attempt.</description></item>
    /// </list>
    ///
    /// <para>
    /// Additional diagnostic fields are included only when relevant:
    /// </para>
    ///
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>"ProcessKillException"</c> — present only when the process was observed to be
    ///       <c>NotExited</c> and a best-effort attempt to terminate it
    ///       (via <see cref="Process.Kill(bool)"/>) failed. The value is the caught exception.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>"ProcessExitCheckException"</c> — present only when the process exit state could not be
    ///       determined because <see cref="Process.HasExited"/> itself threw during the exit check. This
    ///       captures the original exception that made the exit status <c>Indeterminate</c>.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// These fields are useful for diagnosing cases such as processes failing to terminate, OS-level handle
    /// failures, corrupted process state, or unexpected platform behavior. All diagnostic data is attached
    /// without modifying the original exception type, message, stack trace, or cancellation semantics.
    /// </para>
    ///
    /// <para>
    /// The method does not interpret success or failure using <see cref="Process.ExitCode"/>.
    /// The exit code is included in the <see cref="ProcessResult"/>, and the caller decides how to interpret it.
    /// </para>
    /// </remarks>
    ///
    public async Task<ProcessResult> ExecuteAsync(
        string fileName,
        string arguments,
        IDictionary<string, string>? environmentVariables = null,
        int operationTimeoutSeconds = 300,
        CancellationToken cancellationToken = default)
    {
        var operationTimeout = ValidateTimeout(operationTimeoutSeconds);

        using Process process = CreateProcess(fileName, arguments, environmentVariables);
        using ProcessStreamReader stdoutReader = new(process, isErrorStream: false, logger);
        using ProcessStreamReader stderrReader = new(process, isErrorStream: true, logger);

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start process: {fileName}");
        }

        Task<string> stdoutTask = stdoutReader.ReadToEndAsync();
        Task<string> stderrTask = stderrReader.ReadToEndAsync();
        Task exitTask = process.WaitForExitAsync(cancellationToken);

        Task operation = Task.WhenAll(exitTask, stdoutTask, stderrTask);

        try
        {
            await operation.WaitAsync(operationTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // Timeout may be thrown either:
            //   - Case A: before the process had exited, or
            //   - Case B: after the process had already exited, but before streams were fully drained.
            throw HandleTimeout(process, operationTimeout, fileName, arguments);
        }
        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
        {
            // Cancellation was explicitly requested by the caller (not a timeout).
            // OCE may be thrown either:
            //   - Case A: by WaitForExitAsync while the process is still running, or
            //   - Case B: by WaitAsync after the process has already exited.
            HandleCancellation(process, fileName, arguments, oce);
            // 'throw;' here preserves the original stack trace from where the OCE was first thrown,
            // inside WaitAsync or WaitForExitAsync not from this catch block.
            throw;
        }

        // The earlier await on Task.WhenAll(...) guarantees that stdoutTask and stderrTask have already run
        // to completion. The .Result simply retrieves their already-computed output without any blocking.
        var stdout = stdoutTask.Result.TrimEnd();
        var stderr = stderrTask.Result.TrimEnd();

        // Normal completion: the process has exited, and stdout/stderr have fully drained.
        return new ProcessResult(
            process.ExitCode,
            stdout,
            stderr,
            $"{fileName} {arguments}");

        // The `using` declarations at the top ensure that both ProcessStreamReader and Process are disposed on
        // every exit path—normal completion, timeout, or cancellation — unsubscribing handlers and releasing OS 
        // resources.
    }

    public JsonElement ParseJsonOutput(ProcessResult result)
    {
        if (result.ExitCode != 0)
        {
            var error = new ParseError(
                result.ExitCode,
                result.Error,
                result.Command);

            return JsonSerializer.SerializeToElement(
                error,
                ServicesJsonContext.Default.ParseError);
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(result.Output);
            return jsonDocument.RootElement.Clone();
        }
        catch
        {
            var fallback = new ParseOutput(result.Output);
            return JsonSerializer.SerializeToElement(
                fallback,
                ServicesJsonContext.Default.ParseOutput);
        }
    }

    internal record ParseError(
        int ExitCode,
        string Error,
        string Command);

    internal record ParseOutput(
        [property: JsonPropertyName("output")]
        string Output);

    private static TimeSpan ValidateTimeout(int operationTimeoutSeconds)
    {
        if (operationTimeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(operationTimeoutSeconds), "Timeout must be a positive number of seconds.");
        }
        return TimeSpan.FromSeconds(operationTimeoutSeconds);
    }

    /// <summary>
    /// Creates and configures a <see cref="Process"/> for execution with redirected stdout/stderr/stdin and
    /// applied environment variables.
    /// </summary>
    private static Process CreateProcess(string fileName, string arguments, IDictionary<string, string>? environmentVariables)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,

            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,

            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (environmentVariables is not null)
        {
            foreach (var pair in environmentVariables)
            {
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
            }
        }

        return new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
    }

    private TimeoutException HandleTimeout(Process process, TimeSpan timeout, string executablePath, string arguments)
    {
        // Get the pre-kill exit state and any kill error (if termination was attempted).
        (ExitCheckResult exitCheck, Exception? killException) = process.TryKill(logger);
        string command = $"{executablePath} {arguments}";

        TimeoutException exception;

        switch (exitCheck.Status)
        {
            case ExitStatus.NotExited:
                // Timeout occurred before the process had exited: the process itself exceeded the timeout.
                exception = new TimeoutException($"Process execution timed out after {timeout.TotalSeconds} seconds: {command}");
                if (killException is not null)
                {
                    exception.Data["ProcessKillException"] = killException;
                    logger.LogWarning(killException, "Failed to kill process after timeout for command: {Command}", command);
                }
                break;

            case ExitStatus.Exited:
                // Timeout occurred after the process had already exited, but before streams were fully drained.
                exception = new TimeoutException($"Process streams draining timed out after {timeout.TotalSeconds} seconds: {command}");
                break;

            case ExitStatus.Indeterminate:
                // Could not determine exit state due to an exception from Process.HasExited (no kill was attempted).
                exception = new TimeoutException($"Process execution or streams draining timed out after {timeout.TotalSeconds} seconds: {command}");
                exception.Data["ProcessExitCheckException"] = exitCheck.CheckException;
                logger.LogWarning(exitCheck.CheckException, "Could not determine process exit state after the timeout for command: {Command}", command);
                break;

            default:
                throw new InvalidOperationException($"Unexpected exit status: {exitCheck.Status}");
        }

        exception.Data["ProcessName"] = process.SafeName();
        exception.Data["ProcessId"] = process.SafeId();
        exception.Data["ProcessExitStatus"] = exitCheck.Status.ToString();

        return exception;
    }

    private void HandleCancellation(Process process, string executablePath, string arguments, OperationCanceledException exception)
    {
        // Get the pre-kill exit state and any kill error (if termination was attempted).
        (ExitCheckResult exitCheck, Exception? killException) = process.TryKill(logger);
        string command = $"{executablePath} {arguments}";

        switch (exitCheck.Status)
        {
            case ExitStatus.NotExited:
                // Cancellation occurred before the process had exited.
                if (killException is not null)
                {
                    exception.Data["ProcessKillException"] = killException;
                    logger.LogWarning(killException, "Failed to kill process after cancellation for command: {Command}", command);
                }
                break;

            case ExitStatus.Exited:
                // Process already exited
                break;

            case ExitStatus.Indeterminate:
                // Could not determine exit state due to an exception from Process.HasExited (no kill was attempted).
                exception.Data["ProcessExitCheckException"] = exitCheck.CheckException;
                logger.LogWarning(exitCheck.CheckException, "Could not determine process exit state during cancellation for command: {Command}", command);
                break;

            default:
                throw new InvalidOperationException($"Unexpected exit status: {exitCheck.Status}");
        }

        exception.Data["ProcessName"] = process.SafeName();
        exception.Data["ProcessId"] = process.SafeId();
        exception.Data["ProcessExitStatus"] = exitCheck.Status.ToString();

        // we deliberately preserve and rethrow (via 'throw;') the original OCE at call site.
    }

    /// <summary>
    /// Reads either stdout or stderr from a <see cref="Process"/> asynchronously.
    /// Handlers are attached in the constructor, and reading begins when
    /// <see cref="StartReading"/> is called after the process has started.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended usage pattern:
    /// </para>
    /// <list type="number">
    ///   <item><description>Create and configure the <see cref="Process"/> with redirected streams.</description></item>
    ///   <item><description>Construct <see cref="ProcessStreamReader"/> (handlers attach immediately).</description></item>
    ///   <item><description>Start the process.</description></item>
    ///   <item><description>Call <see cref="StartReading"/> to begin event-driven reading.</description></item>
    ///   <item><description>Await the returned task to obtain the full stream content.</description></item>
    /// </list>
    ///
    /// <para>
    /// The reader does not handle cancellation directly - the underlying <see cref="Process"/>
    /// owns the reader threads, which naturally terminate when the process exits or is killed.
    /// This type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Accumulates received lines into a buffer.</description></item>
    ///   <item><description>Completes when <c>e.Data == null</c>, the signal that the stream closed.</description></item>
    ///   <item><description>
    ///     Includes a safety-net completion in <see cref="Dispose"/> so callers cannot hang if
    ///     a close event is never raised (only relevant if used outside <c>ExecuteAsync</c>).
    ///   </description></item>
    /// </list>
    /// </remarks>
    private sealed class ProcessStreamReader : IDisposable
    {
        private readonly Process _process;
        private readonly bool _isErrorStream;
        private readonly ILogger<ExternalProcessService> _logger;
        private readonly StringBuilder _buffer = new();
        private readonly TaskCompletionSource<string> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly DataReceivedEventHandler _handler;
        private bool _readingStarted;
        private bool _disposed;

        public ProcessStreamReader(Process process, bool isErrorStream, ILogger<ExternalProcessService> logger)
        {
            this._process = process ?? throw new ArgumentNullException(nameof(process));
            this._isErrorStream = isErrorStream;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _handler = (_, e) =>
            {
                if (e.Data is null)
                {
                    // Stream closed – finalize accumulated output and complete.
                    _tcs.TrySetResult(_buffer.ToString());
                }
                else
                {
                    _buffer.AppendLine(e.Data);
                }
            };

            if (isErrorStream)
            {
                process.ErrorDataReceived += _handler;
            }
            else
            {
                process.OutputDataReceived += _handler;
            }
        }

        /// <summary>
        /// Begins asynchronous reading of the associated stream.
        /// Must be called only after <see cref="Process.Start"/> has successfully completed.
        /// </summary>
        /// <remarks>
        /// This method does not accept a <see cref="CancellationToken"/> because the underlying
        /// <c>BeginOutputReadLine</c> / <c>BeginErrorReadLine</c> APIs are event-driven and do not
        /// support token-based cancellation.
        ///
        /// Cancellation of the overall external process operation is handled by <c>ExecuteAsync</c>,
        /// which terminates the process on timeout or external cancellation. Once the process exits
        /// (naturally or via kill), the stream read completes automatically.
        ///
        /// If <c>ProcessStreamReader</c> is ever used outside the normal <c>ExecuteAsync</c> workflow,
        /// <see cref="Dispose"/> provides a safety net by completing the task even if the final
        /// <c>e.Data == null</c> callback was never raised.
        /// </remarks>
        /// <returns>A task that completes when the stream has fully drained.</returns>
        public Task<string> ReadToEndAsync()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_readingStarted)
            {
                throw new InvalidOperationException("StartReading has already been called for this reader.");
            }

            _readingStarted = true;

            if (_isErrorStream)
            {
                _process.BeginErrorReadLine();
            }
            else
            {
                _process.BeginOutputReadLine();
            }

            return _tcs.Task;
        }

        /// <summary>
        /// Disposes the stream reader by unsubscribing the data-received handler and ensuring
        /// that the task returned by <see cref="ReadToEndAsync"/> is completed as a safety net.
        /// </summary>
        /// <remarks>
        /// In the normal <c>ExecuteAsync</c> workflow, the read task will already have completed
        /// before <see cref="Dispose"/> runs:
        /// <list type="bullet">
        ///   <item><description>Success path — the stream has fully drained.</description></item>
        ///   <item><description>Timeout or cancellation — <c>ExecuteAsync</c> has already raised an exception.</description></item>
        /// </list>
        ///
        /// In these cases, the fallback completion inside <see cref="Dispose"/> is a harmless no-op.
        /// Its purpose is to guarantee that callers of <see cref="ReadToEndAsync"/> never block indefinitely if
        /// <c>ProcessStreamReader</c> is used outside the intended <c>ExecuteAsync</c> flow and the
        /// final <c>e.Data == null</c> callback is never delivered.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                if (_isErrorStream)
                {
                    _process.ErrorDataReceived -= _handler;
                }
                else
                {
                    _process.OutputDataReceived -= _handler;
                }
            }
            catch (Exception ex)
            {
                // Unsubscribing the handlers is a best-effort cleanup step; log and swallow any exceptions to avoid disposal throwing.
                _logger.LogDebug(
                    ex,
                    "Unsubscribe from {StreamType} stream during disposal was skipped. Process: {ProcessName}, PID: {Pid}.",
                    StreamType,
                    _process.SafeName(),
                    _process.SafeId());
            }

            // Safety net: if the DataReceived handler _handler never observed the final e.Data == null
            // and therefore never completed the read task, force completion here.
            //
            // We intentionally avoid calling ToString() on the shared StringBuilder _buffer to prevent
            // any cross-thread races with AppendLine() inside the event handler. In all normal
            // ExecuteAsync scenarios, the task will already be completed before Dispose() runs,
            // making completion here a no-op.
            //
            // In timeout or cancellation scenarios, the caller receives an exception and does not
            // consume stream output, so try completing with an empty string is sufficient and avoids
            // touching potentially-mutating state.
            if (_tcs.TrySetResult(string.Empty))
            {
                _logger.LogDebug(
                    "ProcessStreamReader for {StreamType} (Process: {ProcessName}, PID: {Pid}) completed during disposal.",
                    StreamType,
                    _process.SafeName(),
                    _process.SafeId());
            }
        }

        private string StreamType => _isErrorStream ? "stderr" : "stdout";
    }
}

/// <summary>
/// Extensions for safe process inspection and kill operations.
/// </summary>
internal static class ProcessExtensions
{
    public static int SafeId(this Process process)
    {
        try
        {
            return process.Id;
        }
        catch
        {
            return -1;
        }
    }

    public static string SafeName(this Process process)
    {
        try
        {
            return process.ProcessName;
        }
        catch
        {
            return "<unknown>";
        }
    }

    /// <summary>
    /// Gets the exit state of a process, defensively handling exceptions that may be thrown by <see cref="Process.HasExited"/>.
    ///
    /// See official docs:
    /// https://learn.microsoft.com/dotnet/api/system.diagnostics.process.hasexited
    /// </summary>
    ///
    /// <param name="process">The process to check.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <returns>
    /// An <see cref="ExitState"/> indicating:
    /// <list type="bullet">
    ///   <item><description><see cref="ExitStatus.Exited"/> with null exception if the process has exited.</description></item>
    ///   <item><description><see cref="ExitStatus.NotExited"/> with null exception if the process has not exited.</description></item>
    ///   <item><description><see cref="ExitStatus.Exited"/> with null exception if <see cref="InvalidOperationException"/> is thrown.</description></item>
    ///   <item><description>
    ///     <see cref="ExitStatus.Indeterminate"/> with the exception if <see cref="System.ComponentModel.Win32Exception"/> or <see cref="NotSupportedException"/> is thrown.
    ///   </description></item>
    /// </list>
    /// </returns>
    public static ExitCheckResult CheckExitState(this Process process, ILogger logger)
    {
        try
        {
            return process.HasExited
                ? new ExitCheckResult(ExitStatus.Exited, CheckException: null)
                : new ExitCheckResult(ExitStatus.NotExited, CheckException: null);
        }
        catch (InvalidOperationException checkException)
        {
            // Official docs: "No process is associated with this object." - treat as "already gone".
            logger.LogDebug(
            checkException,
            "Process.HasExited reported no associated process. Treating as already exited. " +
            "Process: {ProcessName}, PID: {Pid}", process.SafeName(), process.SafeId());
            return new ExitCheckResult(ExitStatus.Exited, CheckException: null);
        }
        catch (System.ComponentModel.Win32Exception checkException)
        {
            // Failure to read status (access denied, invalid handle, etc.).
            return new ExitCheckResult(ExitStatus.Indeterminate, checkException);
        }
        catch (NotSupportedException checkException)
        {
            // Remote process or unsupported scenario – not a valid case for Azure MCP Server.
            return new ExitCheckResult(ExitStatus.Indeterminate, checkException);
        }
    }

    /// <summary>
    /// Best-effort attempt to terminate the process (and its tree), returning the exit state observed <em>before</em>
    /// the kill attempt and any exception from the kill attempt.
    ///
    /// See official docs:
    /// https://learn.microsoft.com/dotnet/api/system.diagnostics.process.Kill
    /// </summary>
    ///
    /// <param name="process">The process to terminate.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    ///
    /// <remarks>
    /// The returned <see cref="ExitCheckResult"/> reflects the state observed <em>before</em> the kill attempt.
    /// If <see cref="ExitStatus.NotExited"/> is reported, this method calls <see cref="Process.Kill(bool)"/>.
    ///
    /// Kill may throw:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="InvalidOperationException"/> – no process is associated with this object (for example, it has
    ///     already exited or the handle is invalid). In this case, the exception is logged as debug information and
    ///     is <b>not</b> treated as a kill failure, since there is nothing left to terminate.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="System.ComponentModel.Win32Exception"/> or
    ///     <see cref="NotSupportedException"/> – treated as genuine kill failures and returned to the caller for
    ///     diagnostic purposes.
    ///   </description></item>
    /// </list>
    /// </remarks>
    public static (ExitCheckResult ExitCheck, Exception? KillException) TryKill(this Process process, ILogger logger)
    {
        var exitCheck = process.CheckExitState(logger);

        if (exitCheck.Status == ExitStatus.NotExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                return (exitCheck, null);
            }
            catch (InvalidOperationException e)
            {
                // Official docs: "No process is associated with this object." - treat as "already gone".
                logger.LogDebug(
                    e,
                    "Process.Kill reported no associated process. Treating as already exited. " +
                    "Process: {ProcessName}, PID: {Pid}", process.SafeName(), process.SafeId());

                // Considered success from a termination perspective: nothing left to kill.
                return (exitCheck, null);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // Failure to terminate (access denied, invalid handle, etc.).
                return (exitCheck, e);
            }
            catch (NotSupportedException e)
            {
                // Remote process or unsupported scenario – not a valid case for Azure MCP Server.
                return (exitCheck, e);
            }
        }

        // Process has already exited or exit state was indeterminate; nothing to kill.
        return (exitCheck, null);
    }

    /// <summary>
    /// Represents the exit status of a process.
    /// </summary>
    public enum ExitStatus
    {
        /// <summary>
        /// The process has exited.
        /// </summary>
        Exited,

        /// <summary>
        /// The process has not exited.
        /// </summary>
        NotExited,

        /// <summary>
        /// The process exit status could not be determined because
        /// <see cref="Process.HasExited"/> threw an exception during the check.
        /// </summary>
        Indeterminate
    }

    /// <summary>
    /// Represents the observed exit state of a process as determined by
    /// <see cref="Process.HasExited"/>.
    ///
    /// This captures:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       The <see cref="ExitStatus"/>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An associated exception only when the status is <see cref="ExitStatus.Indeterminate"/>
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    public record ExitCheckResult(ExitStatus Status, Exception? CheckException);
}
