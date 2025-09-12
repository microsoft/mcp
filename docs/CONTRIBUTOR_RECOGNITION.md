# Automatic Contributor Recognition in CHANGELOG

This repository now automatically recognizes and thanks contributors in the CHANGELOG.md file for each release.

## How it Works

### Automatic Release Updates
When a new release is published on GitHub, the `update-changelog-contributors.yml` workflow automatically:

1. **Extracts contributors** from git commits between the previous release and the current release
2. **Identifies co-authors** from commit messages using "Co-authored-by:" tags
3. **Formats contributor information** for inclusion in the CHANGELOG
4. **Updates the CHANGELOG.md** file with the new release section including contributors
5. **Commits and pushes** the changes back to the repository

### Manual Changelog Generation
You can also manually generate a complete changelog using conventional commits:

1. Go to the **Actions** tab in the GitHub repository
2. Run the **"Generate Changelog"** workflow
3. Optionally specify:
   - **Since**: A specific tag or commit to generate changes from
   - **Include unreleased**: Whether to include unreleased changes

This will create a Pull Request with the generated CHANGELOG.md.

## Configuration

### Git Cliff Configuration
The changelog generation uses `git-cliff` with configuration in `cliff.toml`. This supports:

- **Conventional Commits**: Automatically categorizes changes based on commit prefixes
- **GitHub Integration**: Pulls contributor information from GitHub API
- **Custom Grouping**: Organizes changes into logical sections (Features, Bug Fixes, etc.)

### Supported Commit Conventions
Use these prefixes in your commit messages for automatic categorization:

- `feat:` - 🚀 Features
- `fix:` - 🐛 Bug Fixes  
- `docs:` - 📚 Documentation
- `perf:` - ⚡ Performance
- `refactor:` - 🚜 Refactor
- `style:` - 🎨 Styling
- `test:` - 🧪 Testing
- `chore:` - ⚙️ Miscellaneous Tasks
- `ci:` - ⚙️ Miscellaneous Tasks
- `revert:` - ◀️ Revert

### Co-author Recognition
To give credit to co-authors, include this in your commit message:
```
Co-authored-by: Name <email@example.com>
```

## Examples

### Release with Contributors
```markdown
## [1.2.0] - 2024-01-15

### Features
- Added new MCP server functionality

### Contributors 👥

Thanks to all the contributors who made this release possible:

- [@username1](https://github.com/username1) - *first-time contributor* 🎉
- [@username2](https://github.com/username2)
- John Doe
```

### Manual Testing
You can test contributor extraction locally using:
```bash
# Test contributors since last tag
./test-contributors.sh

# Test contributors since specific tag
./test-contributors.sh v1.0.0
```

## Benefits

1. **Community Recognition**: Automatically thanks all contributors
2. **Reduced Conflicts**: Avoids manual CHANGELOG editing conflicts
3. **Consistency**: Uses standard formatting and conventions
4. **Automation**: Reduces manual maintenance overhead
5. **GitHub Integration**: Leverages GitHub's contributor information

## Integration with Release Process

The contributor recognition integrates seamlessly with the existing Azure DevOps release pipelines. When releases are created via `gh release create`, the GitHub webhook triggers the contributor update workflow automatically.