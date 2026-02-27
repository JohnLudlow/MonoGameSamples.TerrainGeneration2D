Agent Safety and Enforcement Policies
=====================================

Purpose
-------

This document describes practical, enforceable patterns to prevent interactive or automated agents (Copilot CLI agents, Copilot in-editor assistants, or other LLM agents) from making unauthorized repository changes. It focuses on two goals:

1. Preventing agents from writing or modifying files outside approved patterns.
2. Preventing agents from committing or pushing changes without explicit human approval.

Threat model
------------

- Agents may be configured with guidance (front-matter) such as `forbidden_paths` or `allowed_write_paths` but these are advisory and not always enforced by every client (CLI vs IDE vs service).
- The authoritative enforcement layer is CI/workflow permissions and repository policy (branch protection, required reviewers, limited GITHUB_TOKEN scopes).

Recommended patterns (summary)
------------------------------

1. Treat front-matter (for example `forbidden_paths`) as documentation and guidance only. Do not rely on it as the only control.
2. Always enforce write/commit restrictions in CI using a validator (for example `.github/scripts/pr-validator.js`) and fail the run when violations are found (strict mode).
3. Run agents in dry-run mode by default. Configure workflows to upload artifacts (reports, proposed edits) rather than committing automatically.
4. Gate any commit-or-PR creation behind a manual approval step or a separate, strongly-scoped workflow that requires an explicit input or maintainer trigger.
5. Use branch protection and required reviews to prevent automatic merges of agent-created branches.
6. Keep tokens minimal: prefer read-only tokens for regular agent runs; only enable write-scoped tokens for an explicitly reviewed workflow.

Concrete enforcement recipe
--------------------------

- Agent configuration: keep `forbidden_paths` in front-matter to communicate intent to humans and IDEs, but expect the CLI or service to ignore unknown keys. Example front-matter keys supported by GitHub docs: `name`, `description`, `tools`, `mcp-servers`, `forbidden_paths`.

- CI validator (recommended):
  - Add a workflow that triggers on pull_request and runs a Node/Powershell validator (e.g., `.github/scripts/pr-validator.js`).
  - Validator checks changed files and aborts (set non-zero exit) when an agent has modified disallowed files.
  - In dry-run development you can log warnings; to enforce, remove tolerant fallbacks (e.g., `|| true`) and exit non-zero on issues.

- Dry-run artifact workflow (example):
  - Agents run in CI and produce `./test-results/agent-pr-validation.md` and other artifacts.
  - The workflow uploads artifacts and appends the report to `$GITHUB_STEP_SUMMARY` for fast review.
  - No commits or branch creation occurs in this flow.

- Explicit commit/PR workflow (opt-in):
  - Create a separate workflow that is only triggered manually (`workflow_dispatch`) or by a maintainer label.
  - This workflow runs stricter checks, and if the validation passes, it can create a branch and open a PR using a scoped write token.
  - Require a human review before merging (branch protection rules, required reviewers, and status checks).

- Branch protection & policies:
  - Require status checks (agent validator) to pass before merging.
  - Require at least one human review for PRs created by automation.
  - Disable `pull_request` triggers for certain branches if necessary.

- Avoid infinite loops and accidental writes:
  - When creating commits in workflow, mark commit messages and actor (e.g., `github-actions[bot]`) and skip workflow runs triggered by that actor (use `if` guards: `if: github.actor != 'github-actions[bot]'`).
  - Use distinct branch prefixes for agent-generated branches (e.g., `agent-output/<run-id>`).

How to move from dry-run to enforcement
---------------------------------------

1. Run the validator in dry-run and inspect reports (artifacts + step summary).
2. Once confident, flip the validator to strict mode:
   - Remove `|| true` from workflow steps that call the validator.
   - In the validator script replace warnings with failing exit codes (process.exit(2)).
3. Add a protected workflow for opt-in commit/PR creation with a scoped PAT and manual trigger.

Operational notes
-----------------

- Copilot/CLI differences: some clients (CLI) may be stricter about manifest fields and unknown keys; keep manifests conformant to the official schema to avoid runtime errors.
- Human-in-the-loop: always prefer a reviewable PR when production code is affected. Use documentation and artifact reports to make agent output easy to review.

Example quick checklist for repo owners
--------------------------------------

- [ ] Add `pr-validator.js` to `.github/scripts/` and wire it into `pull_request` workflows.
- [ ] Make the validator produce a Markdown artifact and append to `$GITHUB_STEP_SUMMARY`.
- [ ] Start with dry-run (warnings only) until behavior is stable.
- [ ] After validation, switch validator to fail the job on issues and enable branch protections.
- [ ] Implement an opt-in `workflow_dispatch` job that can create PRs with a scoped token and requires manual approval to merge.

If you’d like, can add:

- a sample strict-enforcement variant of the validator and workflow steps, and
- a sample `workflow_dispatch` YAML that demonstrates safe PR creation with a scoped token and required human approval.
