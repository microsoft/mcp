# Support

## How to use the Microsoft Fabric MCP Server

The Microsoft Fabric MCP Server is a comprehensive AI-ready toolkit that provides your AI agents with deep knowledge of Microsoft Fabric APIs, item definitions, and best practices. Build data analytics solutions, author Fabric items, and generate production-ready code directly from VS Code, other coding tools, or AI agents like GitHub Copilot.

Since this is a **local-first server** that runs entirely on your machine, you maintain complete control over your development workflow while giving AI agents the context they need to generate robust, Fabric-compatible code.

**Key Resources:**
- [Getting Started Guide][fabric-readme] - Complete setup and usage instructions
- [Microsoft Fabric Documentation][fabric-docs] - Comprehensive platform documentation
- [Troubleshooting Guide][fabric-troubleshooting] - Common issues and solutions

## How to file issues and get help

This project uses [GitHub Issues][gh-issue] to track bugs and feature requests. Please search the [existing issues][exist-issue] before filing new issues to avoid duplicates.

**Before filing an issue:**
1. **📖 Read our documentation**: Check our [README][fabric-readme] and [TROUBLESHOOTING][fabric-troubleshooting] guides
2. **🔍 Search existing issues**: Someone may have already reported your issue

**Ways to get help:**
- **🐛 Bug Reports**: Use [GitHub Issues][gh-issue] for unexpected behavior or errors
- **❓ Questions**: Use [GitHub Issues][gh-issue] for usage questions and help
- **💡 Feature Requests**: Use [GitHub Issues][gh-issue] with the feature request template
- **🤝 Contributing**: See our [CONTRIBUTING doc][contribute] to help improve the project

### Issue Templates

When filing issues, please use the appropriate template:

- **🐛 Bug Report**: For unexpected behavior or errors
- **✨ Feature Request**: For new functionality or improvements
- **📚 Documentation**: For documentation improvements
- **❓ Question**: For usage questions and help

### What to Include in Bug Reports

To help us resolve issues quickly, please include:

**System Information:**
- Operating system and version
- .NET version (run `dotnet --version`)
- MCP client type and version (VS Code, Claude Desktop, etc.)

**Issue Details:**
- Complete error messages and stack traces
- Steps to reproduce the problem
- Your MCP configuration (redacted for sensitive information)
- Expected vs. actual behavior
- Screenshots or logs if applicable

**Additional Context:**
- Was this working previously?
- Any recent changes to your setup?
- Other MCP servers running concurrently?

## Microsoft Support Policy

The Microsoft Fabric MCP Server is an **open-source project in Public Preview**. Support is provided through community channels and GitHub repositories.

**Community Support (Primary):**
- [GitHub Issues][gh-issue] - Bug reports, feature requests, and questions
- [Microsoft Fabric Community][fabric-community] - Broader Fabric platform questions

**Enterprise Support (Limited):**
Microsoft customers with support agreements may receive assistance with:
- General Microsoft Fabric platform questions
- Fabric REST API documentation and usage guidance
- .NET development and runtime environment issues

> **Note**: Support for the specific MCP Server implementation is primarily through the open-source community channels listed above.

## Public Preview Considerations

As a **Public Preview** project, please note:

- **🔄 Breaking Changes**: Implementation may change before General Availability
- **🚧 Feature Completeness**: Some features are still being developed
- **⚖️ Stability**: While functional, the software is still being refined
- **📚 Documentation**: Some documentation may be incomplete or evolving
- **🔧 API Changes**: Embedded API specifications may be updated frequently

**Your feedback during this preview period is crucial for shaping the final product!**

We especially welcome feedback on:
- Missing Fabric workload types or APIs
- Performance and reliability issues
- Documentation clarity and completeness
- Integration with different MCP clients

## Microsoft Fabric Resources

### Documentation
- [Microsoft Fabric documentation][fabric-docs] - Comprehensive platform documentation
- [Fabric REST API reference][fabric-api-docs] - Official API documentation
- [Fabric developer guide][fabric-dev-docs] - Developer resources and tutorials

### Community
- [Microsoft Fabric Community][fabric-community] - Official community forums
- [Fabric Blog][fabric-blog] - Latest announcements and tutorials
- [Fabric GitHub][fabric-github] - Official Fabric repositories

### Training
- [Microsoft Learn - Fabric][fabric-learn] - Free training modules
- [Fabric Learning Path][fabric-learning-path] - Structured learning content

## Troubleshoot the Fabric MCP Server

If you encounter an unexpected issue while working with the Fabric MCP Server, check out our comprehensive [Troubleshooting Guide][fabric-troubleshooting] for common solutions and diagnostic steps.

**Quick troubleshooting checklist:**
1. ✅ **Verify .NET 9 SDK** is installed: `dotnet --version`
2. ✅ **Check MCP client configuration** syntax and file paths
3. ✅ **Ensure server builds** without errors: `dotnet build`
4. ✅ **Test with simple commands** first to isolate issues
5. ✅ **Check logs** for detailed error information
6. ✅ **Restart your MCP client** after configuration changes
7. ✅ **Verify file permissions** on project directories

**Common Issues:**
- Empty console window (this is normal - server uses stdio)
- .NET 9 runtime not found
- MCP client not detecting tools
- Path resolution problems in configuration

## Contributing to the Project

We welcome contributions! The Fabric MCP Server is part of the broader Microsoft MCP initiative and benefits from community involvement.

**Ways to contribute:**
- 🛠️ **Code**: Fix bugs, add features, improve performance
- 📝 **Documentation**: Improve guides, add examples, fix typos
- 🎯 **Templates**: Add new item definition templates and schemas
- 📚 **Best Practices**: Contribute guidance, examples, and patterns
- 🧪 **Testing**: Report bugs, test new features, verify compatibility
- 💡 **Ideas**: Suggest improvements and new workload support
- 🌐 **Translations**: Help localize documentation (future)

**Getting started with contributions:**
1. Read our [CONTRIBUTING guide][contribute] for detailed instructions
2. Look for "good first issue" labels in [GitHub Issues][gh-issue]
3. Review open issues to understand current development priorities
4. Set up your development environment (see Development Setup below)

### Development Setup

**Prerequisites:**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

**Build steps:**
```bash
# Clone repository
git clone https://github.com/microsoft/mcp
cd mcp

# Build Fabric MCP Server
dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj

# Run tests (if available)
dotnet test tools/Fabric.Mcp.Tools.PublicApi/tests/
```

**Development and debugging:**

1. **Run with verbose logging:**
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --verbosity diagnostic
   ```

2. **Debug in VS Code:**
   - Open the repository in VS Code
   - Set breakpoints in Fabric MCP Server code
   - Use F5 to start debugging

3. **Test individual tools** (if supported):
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- platform get-platform-apis
   ```

## License and Legal

This project is licensed under the MIT License - see the [LICENSE][license] file for details.

**Trademarks**: This project may contain trademarks or logos for projects, products, or services. Use of Microsoft trademarks or logos must follow [Microsoft's Trademark & Brand Guidelines][trademark-guidelines].

---

**Ready to build amazing Fabric experiences with AI?** 

🌟 Star the repository to stay updated  
 Try the server with your favorite AI assistant  

Together, we're building the future of AI-assisted data analytics development!

[gh-issue]: https://github.com/microsoft/mcp/issues/new/choose
[exist-issue]: https://github.com/microsoft/mcp/issues
[fabric-readme]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/README.md
[fabric-troubleshooting]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md
[contribute]: https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md
[license]: https://github.com/microsoft/mcp/blob/main/LICENSE

[fabric-docs]: https://learn.microsoft.com/fabric/
[fabric-api-docs]: https://learn.microsoft.com/rest/api/fabric/
[fabric-dev-docs]: https://learn.microsoft.com/fabric/
[fabric-community]: https://community.fabric.microsoft.com/
[fabric-blog]: https://blog.fabric.microsoft.com/
[fabric-learn]: https://learn.microsoft.com/training/paths/get-started-fabric/
[fabric-learning-path]: https://learn.microsoft.com/training/browse/?products=fabric

[trademark-guidelines]: https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general