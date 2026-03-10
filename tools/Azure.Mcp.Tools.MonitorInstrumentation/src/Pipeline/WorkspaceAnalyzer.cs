using Azure.Mcp.Tools.MonitorInstrumentation.Detectors;
using Azure.Mcp.Tools.MonitorInstrumentation.Generators;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using static Azure.Mcp.Tools.MonitorInstrumentation.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Pipeline;

public class WorkspaceAnalyzer
{
    private readonly IEnumerable<ILanguageDetector> _languageDetectors;
    private readonly IEnumerable<IAppTypeDetector> _appTypeDetectors;
    private readonly IEnumerable<IInstrumentationDetector> _instrumentationDetectors;
    private readonly IEnumerable<IGenerator> _generators;

    public WorkspaceAnalyzer(
        IEnumerable<ILanguageDetector> languageDetectors,
        IEnumerable<IAppTypeDetector> appTypeDetectors,
        IEnumerable<IInstrumentationDetector> instrumentationDetectors,
        IEnumerable<IGenerator> generators)
    {
        _languageDetectors = languageDetectors;
        _appTypeDetectors = appTypeDetectors;
        _instrumentationDetectors = instrumentationDetectors;
        _generators = generators;
    }

    public OnboardingSpec Analyze(string workspacePath)
    {
        // Validate workspace exists
        if (!Directory.Exists(workspacePath))
        {
            return CreateErrorSpec($"Workspace path does not exist: {workspacePath}");
        }

        // Step 1: Detect language
        var languageDetector = _languageDetectors.FirstOrDefault(d => d.CanHandle(workspacePath));
        if (languageDetector == null)
        {
            return CreateErrorSpec("No supported programming language detected in workspace");
        }

        var language = languageDetector.Detect(workspacePath);
        if (language == Language.Unknown)
        {
            return CreateErrorSpec("Could not determine programming language");
        }

        // Step 2: Detect projects
        var appTypeDetector = _appTypeDetectors.FirstOrDefault(d => d.SupportedLanguage == language);
        if (appTypeDetector == null)
        {
            return CreateErrorSpec($"No app type detector available for {language}");
        }

        var projects = appTypeDetector.DetectProjects(workspacePath);
        if (projects.Count == 0)
        {
            return CreateErrorSpec("No projects found in workspace");
        }

        // Step 3: Detect instrumentation state (greenfield/brownfield)
        var instrumentationDetector = _instrumentationDetectors.FirstOrDefault(d => d.SupportedLanguage == language);
        if (instrumentationDetector == null)
        {
            return CreateErrorSpec($"No instrumentation detector available for {language}");
        }

        var instrumentationResult = instrumentationDetector.Detect(workspacePath);

        // Build analysis result
        var analysis = new Analysis
        {
            Language = language,
            Projects = projects,
            State = instrumentationResult.State,
            ExistingInstrumentation = instrumentationResult.ExistingInstrumentation
        };

        // Step 4: Check for multiple instrumentable projects
        // Filter to projects that actually have a matching generator, not just non-Library projects.
        // This avoids blocking when only one project is actually instrumentable (e.g., ASP.NET Core + Console
        // where Console has no generator).
        var instrumentableProjects = projects.Where(p => p.AppType != AppType.Library).ToList();
        if (instrumentableProjects.Count > 1)
        {
            // Check how many of these actually have a generator available
            var projectsWithGenerators = instrumentableProjects.Where(p =>
            {
                var candidateAnalysis = new Analysis
                {
                    Language = language,
                    Projects = [p],
                    State = instrumentationResult.State,
                    ExistingInstrumentation = instrumentationResult.ExistingInstrumentation
                };
                return _generators.Any(g => g.CanHandle(candidateAnalysis));
            }).ToList();

            if (projectsWithGenerators.Count == 1)
            {
                // Only one project has a generator — auto-select it
                analysis = new Analysis
                {
                    Language = language,
                    Projects = projectsWithGenerators,
                    State = instrumentationResult.State,
                    ExistingInstrumentation = instrumentationResult.ExistingInstrumentation
                };
            }
            else if (projectsWithGenerators.Count > 1)
            {
                var filteredAnalysis = new Analysis
                {
                    Language = language,
                    Projects = projectsWithGenerators,
                    State = instrumentationResult.State,
                    ExistingInstrumentation = instrumentationResult.ExistingInstrumentation
                };
                return CreateMultiProjectSpec(filteredAnalysis);
            }
            // If zero have generators, fall through to the "no generator" path below
        }

        // Step 5: Find appropriate generator
        var generator = _generators.FirstOrDefault(g => g.CanHandle(analysis));
        if (generator == null)
        {
            return CreateUnsupportedSpec(analysis);
        }

        // Step 6: Generate spec
        return generator.Generate(analysis);
    }

    private OnboardingSpec CreateErrorSpec(string message)
    {
        var analysis = new Analysis
        {
            Language = Language.Unknown,
            Projects = [],
            State = InstrumentationState.Greenfield,
            ExistingInstrumentation = null
        };

        return new OnboardingSpecBuilder(analysis)
            .WithDecision(Intents.Error, Approaches.None, message)
            .AddWarning(message)
            .BuildWithoutValidation();
    }

    private OnboardingSpec CreateMultiProjectSpec(Analysis analysis)
    {
        var instrumentable = analysis.Projects
            .Where(p => p.AppType != AppType.Library)
            .Select(p => Path.GetFileName(p.ProjectFile))
            .ToList();

        var message = $"Multiple instrumentable projects detected: {string.Join(", ", instrumentable)}. Specify which project to onboard.";

        return new OnboardingSpecBuilder(analysis)
            .WithDecision(Intents.ClarificationNeeded, Approaches.None, message)
            .AddWarning(message)
            .BuildWithoutValidation();
    }

    private OnboardingSpec CreateUnsupportedSpec(Analysis analysis)
    {
        var project = analysis.Projects.FirstOrDefault();
        var appType = project?.AppType ?? AppType.Unknown;
        var message = $"No generator available for {analysis.Language}/{appType}/{analysis.State}";

        return new OnboardingSpecBuilder(analysis)
            .WithDecision(Intents.Unsupported, Approaches.Manual, message)
            .AddWarnings(message, "Manual onboarding required. See Azure Monitor documentation.")
            .BuildWithoutValidation();
    }
}
