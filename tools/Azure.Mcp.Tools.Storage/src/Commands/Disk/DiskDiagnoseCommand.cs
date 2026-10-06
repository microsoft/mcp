// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Disk;
using Azure.Mcp.Tools.Storage.Services;
using Azure.ResourceManager.Compute;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Disk;

[CommandMetadata(
    Id = "65d4c07d-212c-46c9-bf88-6991189aeb6e",
    Name = "diagnose",
    Title = "Diagnose Azure Disk Performance",
    Description = "Diagnoses Azure virtual machine disk performance through the Storage Intelligence service. The built-in endpoint and application scope target Azure public cloud. Identify the target with a standalone VM, canonical VM scale set instance, or attached managed disk resource ID, or with subscription, resource group, and VM name. Diagnose all attached disks or select up to 64 named disks attached to a VM in a resource group. Optionally specify an ISO 8601 time window with explicit UTC offsets of up to 24 hours. Returns disk configuration, performance metrics, throttling intervals, per-LUN analysis, host-side latency metrics when provided by the service, and recommendations.",
    OperationPlane = ToolOperationPlane.Data,
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
    private static readonly ResourceType s_managedDiskResourceType = new("Microsoft.Compute/disks");
    private static readonly ResourceType s_virtualMachineResourceType = new("Microsoft.Compute/virtualMachines");
    private static readonly ResourceType s_virtualMachineScaleSetResourceType = new("Microsoft.Compute/virtualMachineScaleSets");
    private static readonly ResourceType s_virtualMachineScaleSetVmResourceType = new("Microsoft.Compute/virtualMachineScaleSets/virtualMachines");
    private static readonly string[] s_timestampFormats =
    [
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
        "yyyy-MM-dd'T'HH:mm:sszzz",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz"
    ];
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

        if (options.Disk is { Length: > DiskAnalysisRequest.MaxSubResourceIds })
        {
            validationResult.Errors.Add($"--disk accepts at most {DiskAnalysisRequest.MaxSubResourceIds} attached disk names.");
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

        var hasStart = !string.IsNullOrWhiteSpace(options.StartTime);
        var hasEnd = !string.IsNullOrWhiteSpace(options.EndTime);
        if (hasStart != hasEnd)
        {
            validationResult.Errors.Add("--start-time and --end-time must be provided together.");
        }

        var hasValidStart = TryValidateTimestamp(options.StartTime, "--start-time", validationResult, out var start);
        var hasValidEnd = TryValidateTimestamp(options.EndTime, "--end-time", validationResult, out var end);

        if (hasStart && hasEnd && hasValidStart && hasValidEnd && start.HasValue && end.HasValue)
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

        if (!value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.IndexOfAny(['?', '#', '\\']) >= 0
            || value.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment is "." or "..")
            || !ResourceIdentifier.TryParse(value, out var resourceId)
            || resourceId is null
            || !Guid.TryParseExact(resourceId.SubscriptionId, "D", out _)
            || string.IsNullOrWhiteSpace(resourceId.ResourceGroupName)
            || string.IsNullOrWhiteSpace(resourceId.Name))
        {
            error = "The provided value is not a valid Azure resource ID.";
            return false;
        }

        ResourceIdentifier expectedResourceId;
        if (resourceId.ResourceType == s_managedDiskResourceType)
        {
            expectedResourceId = ManagedDiskResource.CreateResourceIdentifier(
                resourceId.SubscriptionId,
                resourceId.ResourceGroupName,
                resourceId.Name);
        }
        else if (resourceId.ResourceType == s_virtualMachineResourceType)
        {
            isVirtualMachine = true;
            expectedResourceId = VirtualMachineResource.CreateResourceIdentifier(
                resourceId.SubscriptionId,
                resourceId.ResourceGroupName,
                resourceId.Name);
        }
        else if (resourceId.ResourceType == s_virtualMachineScaleSetVmResourceType
            && resourceId.Parent is { } parent
            && parent.ResourceType == s_virtualMachineScaleSetResourceType
            && !string.IsNullOrWhiteSpace(parent.Name))
        {
            isVirtualMachine = true;
            expectedResourceId = VirtualMachineScaleSetVmResource.CreateResourceIdentifier(
                resourceId.SubscriptionId,
                resourceId.ResourceGroupName,
                parent.Name,
                resourceId.Name);
        }
        else
        {
            error = "--resource-id must identify a Microsoft.Compute/virtualMachines, Microsoft.Compute/virtualMachineScaleSets/virtualMachines, or Microsoft.Compute/disks resource.";
            return false;
        }

        if (!string.Equals(expectedResourceId.ToString(), resourceId.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            error = "The provided value is not a valid Azure resource ID.";
            return false;
        }

        return true;
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

        if (!HasExplicitTimestampOffset(value)
            || !DateTimeOffset.TryParseExact(
                value,
                s_timestampFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var parsedTimestamp))
        {
            validationResult.Errors.Add($"{optionName} must be a valid ISO 8601 timestamp with an explicit UTC offset.");
            return false;
        }

        timestamp = parsedTimestamp;
        return true;
    }

    private static bool HasExplicitTimestampOffset(string value)
    {
        if (value.EndsWith('Z'))
        {
            return true;
        }

        if (value.Length < 6)
        {
            return false;
        }

        var offset = value.AsSpan(value.Length - 6);
        return offset[0] is '+' or '-'
            && char.IsAsciiDigit(offset[1])
            && char.IsAsciiDigit(offset[2])
            && offset[3] == ':'
            && char.IsAsciiDigit(offset[4])
            && char.IsAsciiDigit(offset[5]);
    }

    public record DiskDiagnoseCommandResult(JsonElement Analysis);
}
