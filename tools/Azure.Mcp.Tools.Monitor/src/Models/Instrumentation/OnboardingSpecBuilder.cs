namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

/// <summary>
/// Builder for creating OnboardingSpec instances with fluent API
/// </summary>
public class OnboardingSpecBuilder
{
    private string _version = OnboardingConstants.SpecVersion;
    private string? _agentMustExecuteFirst;
    private Analysis _analysis = null!;
    private Decision _decision = null!;
    private readonly List<OnboardingAction> _actions = new();
    private readonly List<string> _warnings = new();
    private int _nextOrder = 0;

    public OnboardingSpecBuilder(Analysis analysis)
    {
        _analysis = analysis;
    }

    public OnboardingSpecBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    public OnboardingSpecBuilder WithAgentPreExecuteInstruction(string instruction)
    {
        _agentMustExecuteFirst = instruction;
        return this;
    }

    public OnboardingSpecBuilder WithDecision(string intent, string targetApproach, string rationale)
    {
        _decision = new Decision
        {
            Intent = intent,
            TargetApproach = targetApproach,
            Rationale = rationale
        };
        return this;
    }

    public OnboardingSpecBuilder WithDecision(Decision decision)
    {
        _decision = decision;
        return this;
    }

    public OnboardingSpecBuilder AddReviewEducationAction(
        string id,
        string description,
        List<string> resources,
        params string[] dependsOn)
    {
        _actions.Add(ActionDetailsExtensions.CreateAction(
            id,
            ActionType.ReviewEducation,
            description,
            new ReviewEducationDetails(resources),
            _nextOrder++,
            dependsOn
        ));
        return this;
    }

    public OnboardingSpecBuilder AddPackageAction(
        string id,
        string description,
        string packageManager,
        string package,
        string version,
        string targetProject,
        params string[] dependsOn)
    {
        _actions.Add(ActionDetailsExtensions.CreateAction(
            id,
            ActionType.AddPackage,
            description,
            new AddPackageDetails(packageManager, package, version, targetProject),
            _nextOrder++,
            dependsOn
        ));
        return this;
    }

    public OnboardingSpecBuilder AddModifyCodeAction(
        string id,
        string description,
        string file,
        string codeSnippet,
        string insertAfter,
        string requiredUsing,
        params string[] dependsOn)
    {
        _actions.Add(ActionDetailsExtensions.CreateAction(
            id,
            ActionType.ModifyCode,
            description,
            new ModifyCodeDetails(file, codeSnippet, insertAfter, requiredUsing),
            _nextOrder++,
            dependsOn
        ));
        return this;
    }

    public OnboardingSpecBuilder AddConfigAction(
        string id,
        string description,
        string file,
        string jsonPath,
        string value,
        string? envVarAlternative = null,
        params string[] dependsOn)
    {
        _actions.Add(ActionDetailsExtensions.CreateAction(
            id,
            ActionType.AddConfig,
            description,
            new AddConfigDetails(file, jsonPath, value, envVarAlternative),
            _nextOrder++,
            dependsOn
        ));
        return this;
    }

    public OnboardingSpecBuilder AddManualStepAction(
        string id,
        string description,
        string instructions,
        List<string>? links = null,
        params string[] dependsOn)
    {
        _actions.Add(ActionDetailsExtensions.CreateAction(
            id,
            ActionType.ManualStep,
            description,
            new ManualStepDetails(instructions, links),
            _nextOrder++,
            dependsOn
        ));
        return this;
    }

    public OnboardingSpecBuilder AddAction(OnboardingAction action)
    {
        _actions.Add(action);
        _nextOrder = Math.Max(_nextOrder, action.Order + 1);
        return this;
    }

    public OnboardingSpecBuilder AddWarning(string warning)
    {
        _warnings.Add(warning);
        return this;
    }

    public OnboardingSpecBuilder AddWarnings(params string[] warnings)
    {
        _warnings.AddRange(warnings);
        return this;
    }

    public OnboardingSpec Build()
    {
        var spec = new OnboardingSpec
        {
            Version = _version,
            AgentMustExecuteFirst = _agentMustExecuteFirst,
            Analysis = _analysis,
            Decision = _decision,
            Actions = _actions,
            Warnings = _warnings
        };

        // Validate the spec
        var validator = new OnboardingSpecValidator();
        var result = validator.Validate(spec);

        if (!result.IsValid)
        {
            throw new InvalidOperationException(
                $"Invalid OnboardingSpec: {string.Join(", ", result.Errors)}");
        }

        return spec;
    }

    /// <summary>
    /// Build without validation (use with caution)
    /// </summary>
    public OnboardingSpec BuildWithoutValidation()
    {
        return new OnboardingSpec
        {
            Version = _version,
            AgentMustExecuteFirst = _agentMustExecuteFirst,
            Analysis = _analysis,
            Decision = _decision,
            Actions = _actions,
            Warnings = _warnings
        };
    }

    /// <summary>
    /// Process actions from a generator config and add them to the builder
    /// </summary>
    public OnboardingSpecBuilder AddActionsFromConfig(
        GeneratorConfig config,
        string projectFile,
        string entryPoint,
        string projectDir)
    {
        if (config.Actions == null)
        {
            throw new ArgumentNullException(nameof(config.Actions), "Actions cannot be null.");
        }

        foreach (var action in config.Actions)
        {
            switch (action.Type)
            {
                case "review-education":
                    AddReviewEducationAction(
                        action.Id,
                        action.Title,
                        action.LearningResourceIds!);
                    break;

                case "add-package":
                    AddPackageAction(
                        action.Id,
                        action.Title,
                        action.PackageManager!,
                        action.PackageName!,
                        action.Version!,
                        projectFile,
                        action.DependsOn!);
                    break;

                case "modify-code":
                    var codeTemplate = GeneratorConfigLoader.LoadCodeTemplate(action.CodeTemplate!);
                    AddModifyCodeAction(
                        action.Id,
                        action.Title,
                        entryPoint,
                        codeTemplate,
                        action.InsertLocation!,
                        action.RequiredNamespace!,
                        action.DependsOn!);
                    break;

                case "add-config":
                    AddConfigAction(
                        action.Id,
                        action.Title,
                        Path.Combine(projectDir, action.TargetFile!),
                        action.ConfigPath!,
                        action.DefaultValue!,
                        action.EnvVarName!);
                    break;

                case "manual-step":
                    AddManualStepAction(
                        action.Id,
                        action.Title,
                        action.Instructions ?? action.Title,
                        action.Links);
                    break;

                case "validate-install":
                    AddManualStepAction(
                        action.Id,
                        action.Title,
                        action.Instructions ?? $"Verify that the installation step is complete: {action.Title}",
                        action.Links);
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled action type: {action.Type}");
            }
        }
        return this;
    }
}
