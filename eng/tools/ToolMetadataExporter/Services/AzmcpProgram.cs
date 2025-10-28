using Microsoft.Extensions.Options;
using ToolSelection.Models;

namespace ToolMetadataExporter.Services;

public class AzmcpProgram
{
    private readonly string _toolDirectory;

    public AzmcpProgram(IOptions<AppConfiguration> options)
    {
        _toolDirectory = options.Value.WorkDirectory ?? throw new ArgumentNullException(nameof(AppConfiguration.WorkDirectory));
    }

    public virtual Task<ListToolsResult?> LoadToolsDynamicallyAsync()
    {
        return Utility.LoadToolsDynamicallyAsync(_toolDirectory, false);
    }

    public virtual Task<string> GetVersionAsync()
    {
        return Utility.GetVersionAsync();
    }
}
