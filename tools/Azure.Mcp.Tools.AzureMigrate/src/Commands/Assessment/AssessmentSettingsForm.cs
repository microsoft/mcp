// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// The form (MCP elicitation) that asks the user for the target region, currency and main settings of an assessment. Fields are
/// prefilled with the values the agent passed; optional fields left empty keep the assessment defaults.
/// </summary>
internal static class AssessmentSettingsForm
{
    /// <summary>Key of the form in the input requests of the tool call and in the client's input responses.</summary>
    internal const string InputKey = "assessmentSettings";

    /// <summary>Opaque state echoed back by the client when it retries the tool call with the form response.</summary>
    internal const string RequestState = "assessment-settings";

    internal const string TargetRegion = "targetRegion";
    internal const string Currency = "currency";
    internal const string SavingsOption = "savingsOption";
    internal const string AzureHybridBenefit = "azureHybridBenefit";
    internal const string EnvironmentType = "environmentType";
    internal const string DiscountPercentage = "discountPercentage";
    internal const string ScalingFactor = "scalingFactor";

    internal const string DefaultCurrency = "USD";

    /// <summary>Builds the input-required exception that shows the form, prefilled from <paramref name="options"/>.</summary>
    internal static InputRequiredException CreateRequest(string assessmentName, CreateAssessmentOptions options, IReadOnlyList<string> currencies) =>
        new(
            new Dictionary<string, InputRequest> { [InputKey] = InputRequest.ForElicitation(Build(assessmentName, options, currencies)) },
            RequestState);

    /// <summary>Builds the form, prefilled from <paramref name="options"/>.</summary>
    internal static ElicitRequestParams Build(string assessmentName, CreateAssessmentOptions options, IReadOnlyList<string> currencies) =>
        new()
        {
            Message = $"Confirm the settings for assessment '{assessmentName}'. Optional fields left empty use the assessment defaults.",
            RequestedSchema = new()
            {
                Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
                {
                    [TargetRegion] = new ElicitRequestParams.StringSchema
                    {
                        Title = "Target region",
                        Description = "Azure region to assess for, for example eastus or westeurope.",
                        MinLength = 2,
                        MaxLength = 40,
                        Default = CreateAssessmentCommand.NormalizeRegion(options.TargetRegion) ?? Trimmed(options.TargetRegion),
                    },
                    [Currency] = Choice(
                        "Currency",
                        "Currency for the cost estimates.",
                        currencies,
                        CreateAssessmentCommand.NormalizeCurrency(options.Currency) ?? DefaultCurrency),
                    [SavingsOption] = Choice(
                        "Savings option",
                        "Reserved instances or savings plan to price with. Default: none.",
                        ["None", "RI1Year", "RI3Year", "SavingsPlan1Year", "SavingsPlan3Year"],
                        Trimmed(options.SavingsOption)),
                    [AzureHybridBenefit] = Choice(
                        "Azure Hybrid Benefit (Windows)",
                        "Whether to apply Azure Hybrid Benefit for Windows licences. Default: No.",
                        ["Yes", "No"],
                        Trimmed(options.AzureHybridBenefit)),
                    [EnvironmentType] = Choice(
                        "Environment type",
                        "Production or dev/test pricing. Default: Production.",
                        ["Production", "DevTest"],
                        Trimmed(options.EnvironmentType)),
                    [DiscountPercentage] = new ElicitRequestParams.NumberSchema
                    {
                        Title = "Discount (%)",
                        Description = "Discount on Azure costs, 0 to 100. Default: 0.",
                        Minimum = 0,
                        Maximum = 100,
                        Default = options.DiscountPercentage,
                    },
                    [ScalingFactor] = new ElicitRequestParams.NumberSchema
                    {
                        Title = "Comfort factor",
                        Description = "Compute scaling factor, 1 to 2. Default: 1.",
                        Minimum = 1,
                        Maximum = 2,
                        Default = options.ScalingFactor,
                    },
                },
                Required = [TargetRegion, Currency],
            },
        };

    /// <summary>
    /// Reads the form response from <paramref name="inputResponses"/>. Returns null when the client sent no form response, true when the
    /// user submitted it (its values are copied into <paramref name="options"/>; the command validates them afterwards), or false when
    /// the user declined or cancelled it.
    /// </summary>
    internal static bool? TryApply(IDictionary<string, InputResponse>? inputResponses, CreateAssessmentOptions options)
    {
        if (inputResponses is null || !inputResponses.TryGetValue(InputKey, out InputResponse? response) || response is null)
        {
            return null;
        }

        ElicitResult? result = response.Deserialize(InputResponse.ElicitResultJsonTypeInfo);
        if (result?.IsAccepted is not true)
        {
            return false;
        }

        IDictionary<string, JsonElement> content = result.Content ?? new Dictionary<string, JsonElement>();
        options.TargetRegion = GetString(content, TargetRegion) ?? options.TargetRegion;
        options.Currency = GetString(content, Currency) ?? options.Currency;
        options.SavingsOption = GetString(content, SavingsOption) ?? options.SavingsOption;
        options.AzureHybridBenefit = GetString(content, AzureHybridBenefit) ?? options.AzureHybridBenefit;
        options.EnvironmentType = GetString(content, EnvironmentType) ?? options.EnvironmentType;
        options.DiscountPercentage = GetNumber(content, DiscountPercentage) ?? options.DiscountPercentage;
        options.ScalingFactor = GetNumber(content, ScalingFactor) ?? options.ScalingFactor;
        return true;
    }

    private static ElicitRequestParams.TitledSingleSelectEnumSchema Choice(
        string title, string description, IEnumerable<string> values, string? defaultValue) =>
        new()
        {
            Title = title,
            Description = description,
            OneOf = values.Select(v => new ElicitRequestParams.EnumSchemaOption { Const = v, Title = v }).ToList(),
            Default = values.FirstOrDefault(v => v.Equals(defaultValue, StringComparison.OrdinalIgnoreCase)),
        };

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? GetString(IDictionary<string, JsonElement> content, string key) =>
        content.TryGetValue(key, out JsonElement value) && value.ValueKind == JsonValueKind.String ? Trimmed(value.GetString()) : null;

    // A value that is not a number is returned as NaN so the command's range validation reports it.
    private static double? GetNumber(IDictionary<string, JsonElement> content, string key)
    {
        if (!content.TryGetValue(key, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String when string.IsNullOrWhiteSpace(value.GetString()) => null,
            JsonValueKind.String => double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                ? parsed
                : double.NaN,
            JsonValueKind.Null => null,
            _ => double.NaN,
        };
    }
}
