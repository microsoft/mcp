// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;

public sealed class TestFileUploadCommand(ILogger<TestFileUploadCommand> logger)
    : BaseLoadTestingCommand<LoadTestFileUploadOptions>
{
    private const string CommandTitle = "Upload Test File";
    private readonly ILogger<TestFileUploadCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _testIdOption = OptionDefinitions.LoadTesting.Test;
    private readonly Option<string> _fileNameOption = OptionDefinitions.LoadTesting.FileName;
    private readonly Option<string> _localFilePathOption = OptionDefinitions.LoadTesting.LocalFilePath;
    private readonly Option<string> _fileTypeOption = OptionDefinitions.LoadTesting.FileType;

    public override string Name => "file-upload";

    public override string Description =>
        """
        Uploads a test file (such as JMeter JMX scripts, configuration files, or additional artifacts) to an existing Azure Load Testing test configuration.
        This command allows you to upload JMeter test scripts and supporting files that define your load testing scenarios.
        Common file types include "TEST_SCRIPT" for main test script, "USER_PROPERTIES", "ADDITIONAL_ARTIFACTS", "ZIPPED_ARTIFACTS", "URL_TEST_CONFIG" for supporting resources.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_testIdOption);
        command.Options.Add(_fileNameOption);
        command.Options.Add(_localFilePathOption);
        command.Options.Add(_fileTypeOption);
    }

    protected override LoadTestFileUploadOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValue(_testIdOption);
        options.FileName = parseResult.GetValue(_fileNameOption);
        options.LocalFilePath = parseResult.GetValue(_localFilePathOption);
        options.FileType = parseResult.GetValueOrDefault(_fileTypeOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var loadTestingService = context.GetService<ILoadTestingService>();

            var result = await loadTestingService.UploadTestFileAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.FileName!,
                options.LocalFilePath!,
                options.FileType,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new TestFileUploadCommandResult(result), LoadTestJsonContext.Default.TestFileUploadCommandResult);

            _logger.LogInformation("Successfully uploaded file {FileName} to load test {TestId} in resource {TestResourceName}.",
                options.FileName, options.TestId, options.TestResourceName);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to load test {TestId} in resource {TestResourceName}.",
                options.FileName, options.TestId, options.TestResourceName);
            HandleException(context, ex);
            return context.Response;
        }
    }

    public record TestFileUploadCommandResult(TestFile FileUpload);
}
