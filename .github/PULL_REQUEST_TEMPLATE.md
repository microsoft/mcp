## What does this PR do?
`[Provide a clear, concise description of the changes]`

`[Any additional context, screenshots, or information that helps reviewers]`

## GitHub issue number?
`[Link to the GitHub issue this PR addresses]`

## Pre-merge Checklist
- [ ] Required for All PRs
    - [ ] **Read [contribution guidelines](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md)**
    - [ ] PR title clearly describes the change
    - [ ] Commit history is clean with descriptive messages ([cleanup guide](https://github.com/Azure/azure-powershell/blob/master/documentation/development-docs/cleaning-up-commits.md))
    - [ ] Added comprehensive tests for new/modified functionality
    - [ ] Updated `servers/Azure.Mcp.Server/CHANGELOG.md` and/or `servers/Fabric.Mcp.Server/CHANGELOG.md` for product changes (`features, bug fixes, UI/UX, updated dependencies`)
- [ ] For MCP tool changes:
    - [ ] **One tool per PR**: This PR adds or modifies only one MCP tool for faster review cycles
    - [ ] Updated `servers/Azure.Mcp.Server/README.md` and/or `servers/Fabric.Mcp.Server/README.md` documentation
    - [ ] Updated command list in `/servers/Azure.Mcp.Server/docs/azmcp-commands.md` and/or `/docs/fabric-commands.md`
    - [ ] For new or modified tool descriptions, ran [`ToolDescriptionEvaluator`](https://github.com/microsoft/mcp/blob/main/eng/tools/ToolDescriptionEvaluator/Quickstart.md) and obtained a score of `0.4` or more and a top 3 ranking for all related test prompts
    - [ ] For new tools associated with Azure services or publicly available tools/APIs/products, add URL to documentation in the PR description
- [ ] Extra steps for **Azure MCP Server** tool changes:
    - [ ] Updated test prompts in `/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
    - [ ] 👉 For Community (non-Microsoft team member) PRs:
        - [ ] **Security review**: Reviewed code for security vulnerabilities, malicious code, or suspicious activities before running tests (`crypto mining, spam, data exfiltration, etc.`)
        - [ ] **Manual tests run**: added comment `/azp run mcp - pullrequest - live` to run *Live Test Pipeline*
    
