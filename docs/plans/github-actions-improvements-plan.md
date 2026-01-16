# GitHub Actions Improvements Plan

## Overview

This plan outlines improvements to the GitHub Actions workflows for the MonoGame Terrain Generation 2D project. The focus is on refining version numbering to use semantic versioning, enhancing auto-reviewing with additional code quality checks, and improving reporting with better visualization and integration.

## Definition of Terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Semantic Versioning (SemVer) | A versioning scheme using MAJOR.MINOR.PATCH format, where increments indicate breaking changes, new features, or bug fixes. | https://semver.org/ |
| Code Coverage | The percentage of code lines executed during tests, measured to ensure test adequacy. | https://en.wikipedia.org/wiki/Code_coverage |
| Auto-reviewing | Automated processes that review code changes for quality, style, and potential issues before human review. | N/A |
| CI/CD | Continuous Integration/Continuous Deployment, practices for automating build, test, and deployment. | https://en.wikipedia.org/wiki/CI/CD |
| Pull Request (PR) | A GitHub feature for proposing changes, allowing review and discussion. | https://docs.github.com/en/pull-requests |
| Artifact | Files generated during a workflow run, such as test reports or build outputs. | https://docs.github.com/en/actions/learn-github-actions/contexts#artifacts |
| Linting | Static analysis to check code for style, errors, and best practices. | https://en.wikipedia.org/wiki/Lint_(software) |

## Requirements

- **Version Numbering**: Implement semantic versioning based on Git tags and commits, replacing the current date-based revision system.
- **Auto-reviewing**: Add automated code quality checks for C# code, including linting, security scans, and PR size limits.
- **Reporting**: Enhance test and coverage reporting with better visualization, integration with external services, and summary improvements.

## Implementation Steps

### Phase 1: Version Numbering Refinement

1. **Research Semantic Versioning Tools**: Evaluate GitVersion or similar tools for automatic version calculation based on Git history.
2. **Update Workflow**: Modify `main.yml` to use GitVersion action instead of custom PowerShell script.
3. **Configure Versioning**: Set up versioning rules in a `GitVersion.yml` file to handle major, minor, patch increments.
4. **Test Versioning**: Run workflow on test branches to verify version numbers are generated correctly.

### Phase 2: Auto-reviewing Enhancements

1. **Add C# Linting**: Integrate dotnet-format or StyleCop for code style checks in a new job.
2. **Security Scanning**: Add a job using CodeQL or similar for vulnerability detection.
3. **PR Size Check**: Implement a check to warn or fail on large PRs using GitHub CLI or custom script.
4. **Reviewdog Integration**: Extend reviewdog usage for C# issues, similar to markdownlint.

### Phase 3: Reporting Improvements

1. **Enhanced Coverage Reporting**: Integrate with services like Codecov or Coveralls for detailed coverage reports.
2. **Test Result Visualization**: Use additional reporters for better test result displays, perhaps integrating with GitHub Checks API.
3. **Build Summary Enhancements**: Add more details to the build summary, including performance metrics if available.
4. **Artifact Management**: Organize artifacts better, perhaps with retention policies.

### Phase 4: Workflow Optimization

1. **Parallel Jobs**: Optimize job dependencies to run checks in parallel where possible.
2. **Caching**: Add caching for dependencies to speed up builds.
3. **Notifications**: Integrate with Slack or email for build status notifications.

## Implementation Considerations

- **Reliability**: Ensure workflows are robust against failures, with proper error handling and retries.
- **Security**: Use secrets for tokens and avoid exposing sensitive information.
- **Performance**: Minimize workflow run times to reduce costs and feedback delays.
- **Maintainability**: Keep workflows modular and well-documented for easy updates.
- **Compatibility**: Ensure changes work across different OS and environments (currently Windows-focused).
- **User Experience**: Provide clear feedback in PRs and commits about what checks are running.

## Testing

- **Workflow Testing**: Use GitHub's workflow dispatch to test changes manually.
- **Branch Testing**: Create feature branches to test versioning and reporting changes.
- **PR Simulation**: Open test PRs to verify auto-reviewing and annotations.
- **Integration Testing**: Ensure all jobs pass together in the full pipeline.
- **Performance Testing**: Measure workflow run times before and after changes.