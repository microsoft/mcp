// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Options.Disk;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Disk;

[CommandMetadata(
    Id = "65d4c07d-212c-46c9-bf88-6991189aeb6e",
    Name = "diagnose",
    Title = "Diagnose Azure Disk Performance",
    Description = "Diagnoses Azure virtual machine disk performance through the Storage Intelligence service. Identify the target with a VM or attached managed disk resource ID, or with subscription, resource group, and VM name. Diagnose all attached disks or select one or more named disks attached to a VM in a resource group. Optionally specify an ISO 8601 time window of up to 24 hours. Returns disk configuration, performance metrics, throttling intervals, per-LUN analysis, host-side latency metrics when provided by the service, and recommendations.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DiskDiagnoseCommand(
    ILogger<DiskDiagnoseCommand> logger,
    IStorageIntelligenceService storageIntelligenceService,
    ISubscriptionResolver subscriptionResolver)
    : AuthenticatedCommand<DiskDiagnoseOptions, DiskDiagnoseCommand.DiskDiagnoseCommandResult>
{
    private const int MaxResourceIdLength = 2048;
    private const int MaxDiskCount = 100;
    private static readonly ResourceType s_managedDiskResourceType = new("Microsoft.Compute/disks");
    private static readonly ResourceType s_virtualMachineResourceType = new("Microsoft.Compute/virtualMachines");
    private readonly ILogger<DiskDiagnoseCommand> _logger = logger;
    private readonly IStorageIntelligenceService _storageIntelligenceService = storageIntelligenceService;
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver;

    public override void PostBindOptions(DiskDiagnoseOptions options)
    {
        base.PostBindOptions(options);
        if (string.IsNullOrWhiteSpace(options.ResourceId))
        {
            options.Subscription = _subscriptionResolver.ResolveSubscription(options.Subscription);
        }
    }

    public override void ValidateOptions(DiskDiagnoseOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var hasResourceId = !string.IsNullOrWhiteSpace(options.ResourceId);
        var hasFriendlySelector = !string.IsNullOrWhiteSpace(options.ResourceGroup) || !string.IsNullOrWhiteSpace(options.Vm);
        var isVirtualMachineResource = false;

        if (hasResourceId && hasFriendlySelector)
        {
            validationResult.Errors.Add("Use either --resource-id or --resource-group with --vm, not both.");
        }
        else if (hasResourceId)
        {
            if (!TryValidateResourceId(options.ResourceId, out isVirtualMachineResource, out var resourceError))
            {
                validationResult.Errors.Add(resourceError);
            }
            if (!string.IsNullOrWhiteSpace(options.Subscription))
            {
                validationResult.Errors.Add("--subscription is only used with --resource-group and --vm; omit it with --resource-id.");
            }
        }
        else
        {
            ValidateFriendlySelector(options, validationResult);
            isVirtualMachineResource = true;
        }

        if (options.Disk is { Length: > MaxDiskCount })
        {
            validationResult.Errors.Add($"--disk accepts at most {MaxDiskCount} attached disk names.");
        }
        else if (options.Disk is not null)
        {
            if (!isVirtualMachineResource)
            {
                validationResult.Errors.Add("--disk can only be used when diagnosing a virtual machine.");
            }
            foreach (var disk in options.Disk)
            {
                if (!IsValidComputeResourceName(disk, 80))
                {
                    validationResult.Errors.Add("Each --disk value must be a valid managed disk name of 1-80 alphanumeric, hyphen, or underscore characters.");
                }
            }
        }

        var hasValidStart = TryValidateTimestamp(options.StartTime, "--start-time", validationResult, out var start);
        var hasValidEnd = TryValidateTimestamp(options.EndTime, "--end-time", validationResult, out var end);

        if (hasValidStart && hasValidEnd && start.HasValue && end.HasValue)
        {
            if (end <= start)
            {
                validationResult.Errors.Add("--end-time must be after --start-time.");
            }
            else if (end - start > TimeSpan.FromHours(24))
            {
                validationResult.Errors.Add("The analysis time window cannot exceed 24 hours.");
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        DiskDiagnoseOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var analysis = await _storageIntelligenceService.DiagnoseDiskAsync(
                options.ResourceId,
                options.Subscription,
                options.ResourceGroup,
                options.Vm,
                options.Disk,
                options.StartTime,
                options.EndTime,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DiskDiagnoseCommandResult(analysis),
                StorageJsonContext.Default.DiskDiagnoseCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error diagnosing disk performance. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static void ValidateFriendlySelector(DiskDiagnoseOptions options, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(options.Subscription))
        {
            validationResult.Errors.Add("--subscription is required with --resource-group and --vm when no default subscription is configured.");
        }
        if (!IsValidResourceGroupName(options.ResourceGroup))
        {
            validationResult.Errors.Add("--resource-group must be a valid Azure resource group name of 1-90 characters.");
        }
        if (!IsValidComputeResourceName(options.Vm, 64, allowPeriod: true))
        {
            validationResult.Errors.Add("--vm must be a valid virtual machine name of 1-64 alphanumeric, hyphen, underscore, or period characters.");
        }
    }

    private static bool TryValidateResourceId(string? value, out bool isVirtualMachine, out string error)
    {
        isVirtualMachine = false;
        error = "";
        if (string.IsNullOrWhiteSpace(value))
        {
            error = "--resource-id is required.";
            return false;
        }

        if (value.Length > MaxResourceIdLength)
        {
            error = $"Azure resource IDs cannot exceed {MaxResourceIdLength} characters.";
            return false;
        }

        try
        {
            var resourceId = new ResourceIdentifier(value);
            isVirtualMachine = resourceId.ResourceType == s_virtualMachineResourceType;
            if (resourceId.ResourceType == s_managedDiskResourceType || isVirtualMachine)
            {
                return true;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            error = "The provided value is not a valid Azure resource ID.";
            return false;
        }

        error = "--resource-id must identify a Microsoft.Compute/virtualMachines or Microsoft.Compute/disks resource.";
        return false;
    }

    private static bool IsValidResourceGroupName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 90 &&
        value[^1] != '.' &&
        value.All(character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.' or '(' or ')');

    private static bool IsValidComputeResourceName(string? value, int maxLength, bool allowPeriod = false) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= maxLength &&
        char.IsLetterOrDigit(value[0]) &&
        (char.IsLetterOrDigit(value[^1]) || value[^1] == '_') &&
        value.All(character => char.IsLetterOrDigit(character) || character is '_' or '-' || (allowPeriod && character == '.'));

    private static bool TryValidateTimestamp(
        string? value,
        string optionName,
        ValidationResult validationResult,
        out DateTimeOffset? timestamp)
    {
        timestamp = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!DateTimeOffset.TryParse(value, out var parsedTimestamp))
        {
            validationResult.Errors.Add($"{optionName} must be a valid ISO 8601 timestamp.");
            return false;
        }

        timestamp = parsedTimestamp;
        return true;
    }

    public record DiskDiagnoseCommandResult(JsonElement Analysis);
}
