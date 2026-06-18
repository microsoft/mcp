# Pipeline Authoring for Releases

## Overview

This document describes best practices for authoring release pipelines, with a focus on the release flow. The pipeline stages progress through validation, signing, and finally publishing of artifacts.

## Release Flow Principles

The release flow is guided by the following requirements, in reverse order of execution:

### 1. Only Release Static Artifacts
- Release jobs should **not create artifacts** during the release stage
- Release jobs must only **download pre-built artifacts** and publish them
- All artifact creation happens in earlier build stages, ensuring immutability through the release process

### 2. Only Release Signed Binaries
- Every signable artifact being published **must be signed**
  - Most artifacts support some level of signing.  (powershell, zip, nuget, dll, exe, etc)
- Unsigned artifacts should be rejected during validation stages
- Signing happens before the release stage in dedicated signing jobs

### 3. Use ESRP as the Preferred Release Mechanism
- **ESRP tasks are the primary method** for publishing artifacts
- Use ESRP tasks for all supported artifact types instead of direct publishing (e.g., npm, pypi)
- This ensures consistent publishing practices and compliance with security requirements

### 4. Validate Packages and Signatures Before Release
- Implement validation stages that verify:
  - Package integrity
  - Signature validity
  - Artifact metadata
- Validation must pass before artifacts proceed to release

### 5. Leverage 1ES Tasks for Validation and Security
- 1ES tasks provide additional validation and security checks
- Use 1ES tasks whenever available for enhanced security posture
- These tasks integrate with security tooling and compliance frameworks

## Pipeline Stage Structure

### Early Stages (Build & Validation)
- The build and validation stages are shared by the pull request and server-specific pipelines
- When run in a pull request, the early stages should support multi-server builds by not requiring a filled ServerName parameter.
- Use the `build_info.json` file and `serverMatrix` to determine which servers are being built
- Jobs should iterate over the configured servers to support multi-platform, multi-server builds

### Release and Signing Stages
- Can be written as single-server stages by depending on a required `ServerName` parameter
- These stages are typically gated and only run on designated release branches
- Use server-specific parameters to control which agents execute these critical jobs
- Should use a `deployment:` job so it can depend on a ADO environment that provides a manual approval gate
- Should not checkout the repo
  - Deployment jobs should only download artifacts and publish them. They shouldn't do any artifact mutation.
  - 1ES release jobs will produce an error if they include a checkout step

## Best Practices

1. **Separation of Concerns**: Keep build, signing, and release stages separate
2. **Artifact Immutability**: Downloaded artifacts should not be modified during release
3. **Consistency**: Always use ESRP for publishing to maintain uniform practices
4. **Validation**: Never skip validation steps; they prevent invalid releases
