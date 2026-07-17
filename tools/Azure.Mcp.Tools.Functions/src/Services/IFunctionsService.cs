// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

public interface IFunctionsService
{
    Task<LanguageListResult> GetLanguageListAsync(CancellationToken cancellationToken = default);

    Task<ProjectTemplateResult> GetProjectTemplateAsync(SupportedLanguages language, CancellationToken cancellationToken = default);

    Task<TemplateListResult> GetTemplateListAsync(SupportedLanguages language, CancellationToken cancellationToken = default);

    Task<FunctionTemplateResult> GetFunctionTemplateAsync(
        SupportedLanguages language,
        string template,
        string? runtimeVersion,
        TemplateOutput output = TemplateOutput.New,
        CancellationToken cancellationToken = default);
}
