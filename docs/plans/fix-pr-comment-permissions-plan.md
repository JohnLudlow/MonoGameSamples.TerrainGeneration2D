# Fix PR Comment Permissions Plan

## Overview

The GitHub Actions workflow is failing with "Resource not accessible by integration" error when attempting to post comments on pull requests for coverage and benchmark reports. This error occurs because the GITHUB_TOKEN lacks write permissions to issues/PRs, especially for pull_request events from forked repositories. Attempts to convert the comment posting from JavaScript (using actions/github-script) to PowerShell (using gh CLI) resulted in a similar GraphQL error: "Resource not accessible by integration (addComment)". This confirms the issue is token permissions, not the implementation method. This plan outlines steps to resolve the permission issue while maintaining security and functionality.

## Definition of Terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| GITHUB_TOKEN | A GitHub-provided token for authentication in workflows, with scoped permissions based on the trigger event. | [GitHub Docs: Automatic token authentication](https://docs.github.com/en/actions/security-guides/automatic-token-authentication) |
| Pull Request Event | A GitHub event triggered by actions on pull requests, such as opening, synchronizing, or reopening. | [GitHub Docs: Events that trigger workflows](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows) |
| Fork | A copy of a repository owned by a different user, used for contributing changes via pull requests. | [GitHub Docs: About forks](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/about-forks) |
| Permissions | Access levels granted to tokens or jobs in GitHub Actions, controlling what actions can be performed (e.g., read/write to contents, issues). | [GitHub Docs: Permissions for GITHUB_TOKEN](https://docs.github.com/en/actions/security-guides/automatic-token-authentication#permissions-for-the-github_token) |
| Personal Access Token (PAT) | A user-generated token with customizable permissions, used for authentication in workflows. | [GitHub Docs: Creating a personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) |

## Requirements

- Enable posting of coverage and benchmark reports as comments on pull requests without errors.
- Maintain security by not exposing sensitive credentials.
- Handle both same-repository and forked pull requests appropriately.
- Ensure the solution is compatible with GitHub's free tier and does not require external services.
- Provide fallback behavior for cases where commenting is not possible.

## Implementation Steps

### Phase 1: Assess Current Permissions and Error Context

1. Confirm the error occurs only on pull_request events, particularly from forks. (Completed: Error persists after converting to gh CLI, indicating token limitation.)
2. Verify that job-level permissions (contents: read, issues: write) are set for test and benchmark jobs.
3. Check if the workflow runs on push events without issues (where GITHUB_TOKEN has more permissions).

### Phase 2: Implement Conditional Comment Posting

1. Modify the composite actions (.github/actions/test/action.yml and .github/actions/benchmark/action.yml) to add a condition checking if the PR is from a fork.
2. Use the expression `github.event.pull_request.head.repo.fork != true` to only attempt commenting on non-forked PRs.
3. If the PR is from a fork, log a message (e.g., "Skipping PR comment due to fork restrictions") and skip the step without failing the job.
4. Test the conditional logic to ensure comments post on same-repo PRs and are skipped on forks.

### Phase 3: Alternative Authentication Method

1. If conditional posting is insufficient, introduce a Personal Access Token (PAT) for repositories where forking is common.
2. Create a repository secret (e.g., PR_COMMENT_TOKEN) with a PAT that has repo scope.
3. Update the composite actions to use the PAT for commenting when GITHUB_TOKEN fails.
4. Ensure the PAT is only used for comment posting and not for other operations.

### Phase 4: Update Workflow Permissions

1. Review and adjust job-level permissions in .github/workflows/main.yml to ensure they are minimal and sufficient.
2. For pr-size job, confirm issues: read is adequate since it only reads PR data.
3. Add workflow-level permissions if needed, but prefer job-level for granularity.

### Phase 5: Testing and Validation

1. Test the workflow on a same-repository PR to ensure comments post successfully.
2. Test on a forked PR to verify graceful handling (e.g., skip commenting with a log message).
3. Monitor for any security warnings or token exposure issues.

## Implementation Considerations

- **Security**: Avoid hardcoding tokens in code; use repository secrets. Limit PAT scopes to necessary permissions only.
- **Reliability**: Ensure that failing to post a comment does not break the build; treat it as a non-critical step. The conversion to gh CLI did not resolve the permission issue, confirming it's token-based.
- **Performance**: Comment posting should not significantly impact job runtime; keep it lightweight.
- **Compatibility**: Solution must work with GitHub's free tier; avoid paid features like GitHub Apps unless necessary.
- **Maintainability**: Document the permission setup in the workflow comments and README.
- **Trade-offs**: Using a PAT increases security risk if mishandled, but conditional skipping may reduce user feedback on forks.
- **Future Features**: Consider integrating with GitHub Checks API for richer reporting instead of comments.

## Testing

- **Unit Tests**: No code changes require unit tests, as this is workflow configuration.
- **Integration Tests**:
  - Create a test PR from the same repository and verify comments appear.
  - Create a test PR from a fork and verify no error occurs, with appropriate logging.
- **Edge Cases**: Test on different event types (push vs. pull_request), and with/without secrets.
- **Validation Criteria**: Workflow runs without "Resource not accessible by integration" errors; comments post when possible; logs indicate skipped actions on forks.
