# GitHub Actions Improvements Plan

## Overview

This plan outlines improvements to the GitHub Actions workflows for the MonoGame Terrain Generation 2D project. The focus is on refining version numbering to use semantic versioning, enhancing auto-reviewing with additional code quality checks, and improving reporting with better visualization and integration.

## Definition of Terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Semantic Versioning (SemVer) | A versioning scheme using MAJOR.MINOR.PATCH format, where increments indicate breaking changes, new features, or bug fixes. | <https://semver.org/> |
| Code Coverage | The percentage of code lines executed during tests, measured to ensure test adequacy. | <https://en.wikipedia.org/wiki/Code_coverage> |
| Auto-reviewing | Automated processes that review code changes for quality, style, and potential issues before human review. | N/A |
| CI/CD | Continuous Integration/Continuous Deployment, practices for automating build, test, and deployment. | <https://en.wikipedia.org/wiki/CI/CD> |
| Pull Request (PR) | A GitHub feature for proposing changes, allowing review and discussion. | <https://docs.github.com/en/pull-requests> |
| Artifact | Files generated during a workflow run, such as test reports or build outputs. | <https://docs.github.com/en/actions/learn-github-actions/contexts#artifacts> |
| Linting | Static analysis to check code for style, errors, and best practices. | <https://en.wikipedia.org/wiki/Lint_(software)> |

## Implementation guide

### Feature requirements

- (***COMPLETE***) Semantic versioning ensures consistent artifact and release versioning
  - GIVEN a GitHub Actions workflow for the project
  - WHEN semantic versioning is implemented
  - THEN build artifacts and releases use MAJOR.MINOR.PATCH format

- (Incomplete) Automated code quality and security checks for pull requests
  - GIVEN a pull request is opened
  - WHEN auto-reviewing jobs run
  - THEN code quality, linting, and security checks are performed automatically

> Partially implemented. Linting and basic security scanning are present, but PR size checks and full reviewdog integration for C# are not yet complete. See Implementation guide Phase 2.

- (Incomplete) Enhanced reporting provides clear feedback on test and coverage status
  - GIVEN tests and coverage jobs are run
  - WHEN reporting is enhanced
  - THEN contributors see clear, visual feedback on test and coverage status

> Coverage reporting and test result visualization are present, but integration with GitHub Checks API and enhanced build summaries are still in progress. See Implementation guide Phase 3.

### Phase 1: Version Numbering Refinement

#### Objective

Implement semantic versioning for build artifacts and releases using GitVersion or similar tools, ensuring MAJOR.MINOR.PATCH format is used consistently.

Success criteria: All build artifacts and releases use correct semantic versioning, and version numbers are generated automatically from Git history.

#### Technical details

1. Research and select a semantic versioning tool (e.g., GitVersion).
2. Update workflow files to use the selected tool.
3. Configure versioning rules in GitVersion.yml.
4. Test versioning on feature and release branches.

#### Phase 1 requirements

- ***COMPLETE*** Semantic versioning is applied to all build artifacts and releases
  - GIVEN a GitHub Actions workflow for the project
  - WHEN semantic versioning is implemented
  - THEN build artifacts and releases use MAJOR.MINOR.PATCH format

#### Examples

```yaml
# Example: Using GitVersion in a workflow
steps:
  - name: Install GitVersion
  uses: GitTools/actions/gitversion/setup@v0.9.10
  - name: Run GitVersion
  uses: GitTools/actions/gitversion/execute@v0.9.10
```

### Phase 2: Auto-reviewing Enhancements

#### Objective

Enhance code quality and security checks in PRs by integrating linting, security scanning, PR size checks, and reviewdog for C# issues.

Success criteria: All PRs are automatically checked for code style, vulnerabilities, and size, with clear feedback provided to contributors.

#### Technical details

1. Integrate dotnet-format or StyleCop for C# linting.
2. Add CodeQL or similar for security scanning.
3. Implement PR size checks using GitHub CLI or scripts.
4. Extend reviewdog integration for C#.

#### Phase 2 requirements

- PRs are automatically checked for code quality, style, and vulnerabilities
  - GIVEN a pull request is opened
  - WHEN auto-reviewing jobs run
  - THEN code quality, linting, and security checks are performed automatically

> Linting and security scanning are implemented, but PR size checks and full reviewdog integration for C# are not yet complete.

#### Examples

```yaml
# Example: Adding dotnet-format to a workflow
steps:
  - name: Run dotnet-format
    run: dotnet format --check
```

### Phase 3: Reporting Improvements

#### Objective

Improve test and coverage reporting with enhanced visualization and integration into PRs and build summaries.

Success criteria: Contributors see clear, visual feedback on test and coverage status in PRs and build summaries.

#### Technical details

1. Integrate Codecov or Coveralls for coverage reporting.
2. Add reporters for test results and integrate with GitHub Checks API.
3. Enhance build summaries with performance metrics and organized artifacts.

#### Phase 3 requirements

- Contributors see clear, visual feedback on test and coverage status
  - GIVEN tests and coverage jobs are run
  - WHEN reporting is enhanced
  - THEN contributors see clear, visual feedback on test and coverage status

> Coverage upload and basic test result reporting are present, but integration with GitHub Checks API and enhanced build summaries are still in progress.

#### Examples

```yaml
# Example: Uploading coverage to Codecov
steps:
  - name: Upload coverage to Codecov
    uses: codecov/codecov-action@v3
    with:
      files: ./coverage.xml
```

### Phase 4: Workflow Optimization

#### Objective

Optimize workflow run times and feedback by running jobs in parallel, caching dependencies, and adding notifications.

Success criteria: Workflow run times are minimized, and contributors receive timely notifications about build status.

#### Technical details

1. Optimize job dependencies for parallel execution.
2. Add caching for dependencies.
3. Integrate notifications with Slack or email.

#### Phase 4 requirements

- Workflow run times are minimized and contributors receive timely notifications
  - GIVEN workflow jobs
  - WHEN jobs are optimized for parallel execution and notifications are added
  - THEN workflow run times are minimized and contributors receive timely notifications

> Some jobs run in parallel and caching is present, but notification integration (e.g., Slack/email) and full optimization are not yet complete.

#### Examples

```yaml
# Example: Caching dependencies in a workflow
steps:
  - name: Cache NuGet packages
    uses: actions/cache@v3
    with:
      path: ~/.nuget/packages
      key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```

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
