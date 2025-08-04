using AzureMcp.Options;

namespace AzureMcp.Areas.Startups.Services
{
    public interface IStartupsCodeGenerationService
    {
        Task<CodeGenerationResult> CreateReactAppAsync(string projectPath, string appName, ReactAppOptions? options = null);
        Task<CodeGenerationResult> CreateStaticWebsiteAsync(string projectPath, string siteName, StaticWebsiteOptions? options = null);
        Task<CodeGenerationResult> CreateFileAsync(string filePath, string content, bool overwrite = false);
        Task<CodeGenerationResult> CreateFolderStructureAsync(string basePath, FolderStructure structure);
        Task<List<string>> GetTemplatesAsync();
        Task<CodeGenerationResult> CreateFromTemplateAsync(string templateName, string projectPath, Dictionary<string, string> parameters);
    }

    public record CodeGenerationResult(
        bool Success,
        string Message,
        List<string> CreatedFiles,
        List<string> CreatedFolders,
        string? ProjectPath = null
    );

    public record ReactAppOptions(
        string? Framework = "create-react-app",
        bool IncludeTypeScript = false,
        bool IncludeRouter = true,
        bool IncludeTailwind = false,
        List<string>? AdditionalPackages = null
    );

    public record StaticWebsiteOptions(
        string? Theme = "modern",
        bool IncludeBootstrap = true,
        bool IncludeJQuery = false,
        string? Title = null,
        string? Description = null
    );

    public record FolderStructure(
        string Name,
        List<FolderStructure>? SubFolders = null,
        List<FileTemplate>? Files = null
    );

    public record FileTemplate(
        string Name,
        string Content,
        string? ContentType = null
    );
}