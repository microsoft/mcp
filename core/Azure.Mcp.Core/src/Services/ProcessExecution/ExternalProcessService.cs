// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Services.ProcessExecution;

public class ExternalProcessService : IExternalProcessService
{
    private readonly Dictionary<string, string> environmentVariables = [];

    public async Task<ProcessResult> ExecuteAsync(
        string executablePath,
        string arguments,
        int timeoutSeconds = 300,
        IEnumerable<string>? customPaths = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(executablePath);

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Executable not found at path: {executablePath}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var keyValuePair in environmentVariables)
        {
            processStartInfo.EnvironmentVariables[keyValuePair.Key] = keyValuePair.Value;
        }

        using var process = new Process { StartInfo = processStartInfo };
        using var outputWaitHandle = new AutoResetEvent(false);
        using var errorWaitHandle = new AutoResetEvent(false);

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data == null)
                outputWaitHandle.Set();
            else
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data == null)
                errorWaitHandle.Set();
            else
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await Task.WhenAll(
            Task.Run(() => process.WaitForExit(timeoutSeconds * 1000)),
            Task.Run(() =>
            {
                outputWaitHandle.WaitOne(1000);
                errorWaitHandle.WaitOne(1000);
            })
        );

        if (!process.HasExited)
        {
            process.Kill();
            throw new TimeoutException($"Process execution timed out after {timeoutSeconds} seconds");
        }

        return new ProcessResult(
            process.ExitCode,
            outputBuilder.ToString().TrimEnd(),
            errorBuilder.ToString().TrimEnd(),
            $"{executablePath} {arguments}");
    }

    public JsonElement ParseJsonOutput(ProcessResult result)
    {
        if (result.ExitCode != 0)
        {
            var error = new ParseError(
                result.ExitCode,
                result.Error,
                result.Command
            );
            return JsonSerializer.SerializeToElement(error, ServicesJsonContext.Default.ParseError);
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(result.Output);
            return jsonDocument.RootElement.Clone();
        }
        catch
        {
            return JsonSerializer.SerializeToElement(new ParseOutput(result.Output), ServicesJsonContext.Default.ParseOutput);
        }
    }

    internal record ParseError(
        int ExitCode,
        string Error,
        string Command
    );

    internal record ParseOutput([property: JsonPropertyName("output")] string Output);

    public void SetEnvironmentVariables(IDictionary<string, string> variables)
    {
        if (variables == null)
        {
            return;
        }

        foreach (var pair in variables)
        {
            environmentVariables[pair.Key] = pair.Value;
        }
    }
}
