// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

public interface IFunctionsService
{
    Task<LanguageListCommandResult> GetLanguageListAsync(CancellationToken cancellationToken = default);

    Task<ProjectGetCommandResult> GetProjectTemplateAsync(string language, CancellationToken cancellationToken = default);

    Task<TemplateListResult> GetTemplateListAsync(string language, CancellationToken cancellationToken = default);

    Task<FunctionTemplateResult> GetFunctionTemplateAsync(string language, string template, string? runtimeVersion, TemplateOutput output = TemplateOutput.New, CancellationToken cancellationToken = default);
}
