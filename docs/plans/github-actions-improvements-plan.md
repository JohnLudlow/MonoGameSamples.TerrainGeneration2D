# GitHub Actions Improvements Plan

## Overview

This plan outlines improvements to the GitHub Actions workflows for the MonoGame Terrain Generation 2D project. The focus is on refining version numbering to use semantic versioning, enhancing auto-reviewing with additional code quality checks, and improving reporting with better visualization and integration.

## Table of contents

- [GitHub Actions Improvements Plan](#github-actions-improvements-plan)
  - [Overview](#overview)
  - [Table of contents](#table-of-contents)
  - [Plan issue](#plan-issue)
  - [Plan status](#plan-status)
  - [Definition of terms](#definition-of-terms)
  - [Architectural considerations and constraints](#architectural-considerations-and-constraints)
  - [Implementation guide](#implementation-guide)
    - [Feature requirements](#feature-requirements)
    - [Phase 1: Version Numbering Refinement](#phase-1-version-numbering-refinement)
      - [Objective](#objective)
      - [Technical details](#technical-details)
      - [Phase 1 requirements](#phase-1-requirements)
      - [Examples](#examples)
    - [Phase 2: Auto-reviewing Enhancements](#phase-2-auto-reviewing-enhancements)
      - [Objective](#objective-1)
      - [Technical details](#technical-details-1)
      - [Phase 2 requirements](#phase-2-requirements)
      - [Examples](#examples-1)
    - [Phase 3: Reporting Improvements](#phase-3-reporting-improvements)
      - [Objective](#objective-2)
      - [Technical details](#technical-details-2)
      - [Phase 3 requirements](#phase-3-requirements)
      - [Examples](#examples-2)
    - [Phase 4: Workflow Optimization](#phase-4-workflow-optimization)
      - [Objective](#objective-3)
      - [Technical details](#technical-details-3)
      - [Phase 4 requirements](#phase-4-requirements)
      - [Examples](#examples-3)
  - [Implementation Considerations](#implementation-considerations)
  - [Testing](#testing)
  - [See also](#see-also)
    - [Related Documentation](#related-documentation)
    - [GitHub Tracking](#github-tracking)
    - [Implementation Files](#implementation-files)
    - [Related Plans](#related-plans)
  - [References](#references)
    - [GitHub Actions](#github-actions)
    - [Versioning](#versioning)
    - [Code Quality Tools](#code-quality-tools)
    - [Coverage and Reporting](#coverage-and-reporting)
    - [CI/CD Best Practices](#cicd-best-practices)
    - [Other Resources](#other-resources)

## Plan issue

This plan currently has no associated GitHub issue. Consider creating an issue to track implementation progress and facilitate collaboration.

## Plan status

- Phase 1: ***COMPLETE*** (Semantic versioning implemented)
- Phase 2: Incomplete (Auto-reviewing partially implemented)
- Phase 3: Incomplete (Reporting partially implemented)
- Phase 4: Incomplete (Optimization partially implemented)

Overall: **In Progress** - 1 of 4 phases complete

## Definition of terms

| Term                         | Meaning                                                                                                                    | Reference                         |
| ---------------------------- | -------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| Artifact                     | Files generated during a workflow run, such as test reports or build outputs                                               | [GitHub Artifacts][ref-artifacts] |
| Auto-reviewing               | Automated processes that review code changes for quality, style, and potential issues before human review                  | N/A                               |
| CI/CD                        | Continuous Integration/Continuous Deployment, practices for automating build, test, and deployment                         | [CI/CD Overview][ref-cicd]        |
| Code Coverage                | The percentage of code lines executed during tests, measured to ensure test adequacy                                       | [Code Coverage][ref-coverage]     |
| Linting                      | Static analysis to check code for style, errors, and best practices                                                        | [Linting][ref-linting]            |
| Pull Request (PR)            | A GitHub feature for proposing changes, allowing review and discussion                                                     | [GitHub PRs][ref-pr]              |
| Semantic Versioning (SemVer) | A versioning scheme using MAJOR.MINOR.PATCH format, where increments indicate breaking changes, new features, or bug fixes | [SemVer][ref-semver]              |

## Architectural considerations and constraints

- **GitHub Actions Platform**: All workflows must run on GitHub-hosted runners (currently Windows-focused)
- **Existing Workflows**: Must maintain backward compatibility with current CI/CD pipelines
- **Secret Management**: All tokens and credentials must be stored as GitHub repository secrets
- **Cost Optimization**: Minimize workflow run times to reduce GitHub Actions usage costs
- **Cross-Platform**: Consider future multi-platform support (Windows, Linux, macOS)
- **Dependency Management**: NuGet package caching and restoration must be reliable
- **Integration Points**: Must integrate with existing build, test, and deployment processes
- **Versioning Strategy**: GitVersion configuration must align with branching strategy (GitFlow/trunk-based)
- **Notification Channels**: Future Slack/email integrations require team communication setup
- **Security Scanning**: CodeQL and security tools must not block legitimate code patterns

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

## See also

### Related Documentation

- **[GitHub Actions Workflows][doc-workflows]** - Current workflow implementations
- **[GitVersion Configuration][doc-gitversion]** - Semantic versioning setup

### GitHub Tracking

- **This Plan**: No associated issue (recommended to create one)

### Implementation Files

- **[.github/workflows/][impl-workflows]** - GitHub Actions workflow definitions
- **[GitVersion.yml][impl-gitversion]** - GitVersion configuration file

### Related Plans

- **[Resolve Build Warnings Plan][plan-warnings]** - CI/CD improvements for build quality

## References

### GitHub Actions

- **[GitHub Actions Documentation][ref-gh-actions]** - Official GitHub Actions docs
- **[Workflow Syntax][ref-workflow-syntax]** - YAML syntax for workflows
- **[GitHub Checks API][ref-checks-api]** - API for status checks and annotations

### Versioning

- **[Semantic Versioning][ref-semver]** - SemVer specification
- **[GitVersion][ref-gitversion]** - GitVersion documentation

### Code Quality Tools

- **[dotnet-format][ref-dotnet-format]** - .NET code formatter
- **[CodeQL][ref-codeql]** - GitHub's code analysis engine
- **[Reviewdog][ref-reviewdog]** - Automated code review tool

### Coverage and Reporting

- **[Codecov][ref-codecov]** - Code coverage service
- **[Coveralls][ref-coveralls]** - Alternative coverage service

### CI/CD Best Practices

- **[CI/CD Overview][ref-cicd]** - Introduction to CI/CD practices
- **[GitHub Actions Best Practices][ref-gh-best-practices]** - Official best practices guide

### Other Resources

- **[GitHub Artifacts][ref-artifacts]** - Artifact storage and retrieval
- **[GitHub PRs][ref-pr]** - Pull request documentation
- **[Code Coverage][ref-coverage]** - Code coverage concepts
- **[Linting][ref-linting]** - Static analysis overview

<!-- Link References -->

<!-- Documentation -->
[doc-workflows]: ../../.github/workflows/
[doc-gitversion]: ../../GitVersion.yml

<!-- Implementation Files -->
[impl-workflows]: ../../.github/workflows/
[impl-gitversion]: ../../GitVersion.yml

<!-- Related Plans -->
[plan-warnings]: resolve-build-warnings-plan.md

<!-- External References - GitHub Actions -->
[ref-gh-actions]: https://docs.github.com/en/actions
[ref-workflow-syntax]: https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions
[ref-checks-api]: https://docs.github.com/en/rest/checks
[ref-gh-best-practices]: https://docs.github.com/en/actions/learn-github-actions/best-practices-for-github-actions

<!-- External References - Versioning -->
[ref-semver]: https://semver.org/
[ref-gitversion]: https://gitversion.net/

<!-- External References - Code Quality -->
[ref-dotnet-format]: https://github.com/dotnet/format
[ref-codeql]: https://codeql.github.com/
[ref-reviewdog]: https://github.com/reviewdog/reviewdog

<!-- External References - Coverage -->
[ref-codecov]: https://about.codecov.io/
[ref-coveralls]: https://coveralls.io/

<!-- External References - CI/CD -->
[ref-cicd]: https://en.wikipedia.org/wiki/CI/CD
[ref-artifacts]: https://docs.github.com/en/actions/learn-github-actions/contexts#artifacts
[ref-pr]: https://docs.github.com/en/pull-requests
[ref-coverage]: https://en.wikipedia.org/wiki/Code_coverage
[ref-linting]: https://en.wikipedia.org/wiki/Lint_(software)
