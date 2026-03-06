// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionsTemplate.Models;

namespace Azure.Mcp.Tools.FunctionsTemplate.Services;

public interface IFunctionTemplatesService
{
    Task<LanguageListResult> GetLanguageListAsync(CancellationToken cancellationToken = default);

    Task<ProjectTemplateResult> GetProjectTemplateAsync(string language, string? runtimeVersion, CancellationToken cancellationToken = default);

    Task<TemplateListResult> GetTemplateListAsync(string language, CancellationToken cancellationToken = default);

    Task<FunctionTemplateResult> GetFunctionTemplateAsync(string language, string template, string? runtimeVersion, CancellationToken cancellationToken = default);
}
