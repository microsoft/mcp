namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Validates onboarding specifications for correctness and consistency
/// </summary>
public class OnboardingSpecValidator
{
    public ValidationResult Validate(OnboardingSpec spec)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate version
        if (string.IsNullOrWhiteSpace(spec.Version))
        {
            errors.Add("Version is required");
        }

        // Validate analysis
        if (spec.Analysis == null)
        {
            errors.Add("Analysis is required");
        }

        // Validate decision
        if (spec.Decision == null)
        {
            errors.Add("Decision is required");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(spec.Decision.Intent))
                errors.Add("Decision.Intent is required");
            if (string.IsNullOrWhiteSpace(spec.Decision.TargetApproach))
                errors.Add("Decision.TargetApproach is required");
            if (string.IsNullOrWhiteSpace(spec.Decision.Rationale))
                errors.Add("Decision.Rationale is required");
        }

        // Validate actions
        ValidateActions(spec.Actions, errors, warnings);

        return new ValidationResult(errors, warnings);
    }

    private void ValidateActions(List<OnboardingAction> actions, List<string> errors, List<string> warnings)
    {
        if (actions == null || actions.Count == 0)
        {
            // Empty actions is valid for error/unsupported specs
            return;
        }

        var actionIds = new HashSet<string>();
        var orders = new HashSet<int>();

        foreach (var action in actions)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(action.Id))
            {
                errors.Add("Action Id is required");
                continue;
            }

            // Check for duplicate IDs
            if (!actionIds.Add(action.Id))
            {
                errors.Add($"Duplicate action ID: {action.Id}");
            }

            // Validate description
            if (string.IsNullOrWhiteSpace(action.Description))
            {
                errors.Add($"Action '{action.Id}' is missing a description");
            }

            // Check for duplicate orders
            if (!orders.Add(action.Order))
            {
                warnings.Add($"Duplicate action order: {action.Order}");
            }

            // Validate dependencies exist
            foreach (var dep in action.DependsOn)
            {
                if (!actions.Any(a => a.Id == dep))
                {
                    errors.Add($"Action '{action.Id}' depends on non-existent action '{dep}'");
                }
            }

            // Validate action details based on type
            ValidateActionDetails(action, errors, warnings);
        }

        // Check for circular dependencies
        DetectCircularDependencies(actions, errors);

        // Check for order gaps
        var sortedOrders = orders.OrderBy(o => o).ToList();
        for (int i = 0; i < sortedOrders.Count - 1; i++)
        {
            if (sortedOrders[i + 1] - sortedOrders[i] > 1)
            {
                warnings.Add($"Gap in action ordering: {sortedOrders[i]} to {sortedOrders[i + 1]}");
            }
        }
    }

    private void ValidateActionDetails(OnboardingAction action, List<string> errors, List<string> warnings)
    {
        switch (action.Type)
        {
            case ActionType.AddPackage:
                ValidateRequiredKey(action, "packageManager", errors);
                ValidateRequiredKey(action, "package", errors);
                ValidateRequiredKey(action, "version", errors);
                ValidateRequiredKey(action, "targetProject", errors);
                break;

            case ActionType.ModifyCode:
                ValidateRequiredKey(action, "file", errors);
                ValidateRequiredKey(action, "codeSnippet", errors);
                ValidateRequiredKey(action, "insertAfter", errors);
                ValidateRequiredKey(action, "requiredUsing", errors);
                break;

            case ActionType.AddConfig:
                ValidateRequiredKey(action, "file", errors);
                ValidateRequiredKey(action, "jsonPath", errors);
                ValidateRequiredKey(action, "value", errors);
                break;

            case ActionType.ReviewEducation:
                ValidateRequiredKey(action, "resources", errors);
                if (action.Details.TryGetValue("resources", out var resources))
                {
                    if (resources is not List<string> list || list.Count == 0)
                    {
                        errors.Add($"Action '{action.Id}' must have at least one resource");
                    }
                }
                break;

            case ActionType.ManualStep:
                ValidateRequiredKey(action, "instructions", errors);
                break;

            default:
                warnings.Add($"Action '{action.Id}' has unrecognized type '{action.Type}' — no detail validation applied");
                break;
        }
    }

    private void ValidateRequiredKey(OnboardingAction action, string key, List<string> errors)
    {
        if (!action.Details.ContainsKey(key) || action.Details[key] == null)
        {
            errors.Add($"Action '{action.Id}' is missing required detail: {key}");
        }
    }

    private void DetectCircularDependencies(List<OnboardingAction> actions, List<string> errors)
    {
        var graph = actions.ToDictionary(a => a.Id, a => a.DependsOn);

        foreach (var action in actions)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();

            if (HasCircularDependency(action.Id, graph, visited, stack))
            {
                errors.Add($"Circular dependency detected involving action '{action.Id}'");
            }
        }
    }

    private bool HasCircularDependency(
        string actionId,
        Dictionary<string, List<string>> graph,
        HashSet<string> visited,
        HashSet<string> stack)
    {
        if (stack.Contains(actionId))
        {
            return true;
        }

        if (visited.Contains(actionId))
        {
            return false;
        }

        visited.Add(actionId);
        stack.Add(actionId);

        if (graph.TryGetValue(actionId, out var dependencies))
        {
            foreach (var dep in dependencies)
            {
                if (HasCircularDependency(dep, graph, visited, stack))
                {
                    return true;
                }
            }
        }

        stack.Remove(actionId);
        return false;
    }
}

/// <summary>
/// Result of validation
/// </summary>
public record ValidationResult
{
    public List<string> Errors { get; init; }
    public List<string> Warnings { get; init; }
    public bool IsValid => Errors.Count == 0;

    public ValidationResult(List<string> errors, List<string> warnings)
    {
        Errors = errors;
        Warnings = warnings;
    }
}
