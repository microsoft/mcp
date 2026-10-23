// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

public interface ILanguageDetector
{
    bool CanHandle(string workspacePath);
    Language Detect(string workspacePath);
}

public interface IAppTypeDetector
{
    Language SupportedLanguage { get; }
    List<ProjectInfo> DetectProjects(string workspacePath);
}

public interface IInstrumentationDetector
{
    Language SupportedLanguage { get; }
    InstrumentationResult Detect(string workspacePath);
}

public record InstrumentationResult(
    InstrumentationState State,
    ExistingInstrumentation? ExistingInstrumentation
);
