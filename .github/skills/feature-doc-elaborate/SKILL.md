---
name: feature-doc-elaborate
description: Produce a feature/component documentation that adheres to repo standards.
---

# Feature Elaboration

## When to use this skill

Use this skill when the user wants to describe a new feature.

## Process inputs

- A description of the requested feature
- Optionally, a document partially describing the feature

## Process outputs

- A detailed document describing the feature including architectural considerations and an implementation guide

## How to generate the new feature document

- Create or update a Markdown Document under [docs](../../docs/features) using the [provided template](../../../docs/plans/feature-template.md).

- Link from [docs/README.md](../docs/README.md) and validate links.

- Inputs: feature name and description, config keys.
- Outputs: new/updated doc path; docs index entry.
- Preconditions: gather architecture/context; decide doc location.
- Verification: link check passes; content follows principles; examples compile.
- Rollback: revert doc if requirements unmet; fix and re-run.
- Naming: place feature docs at `docs/features/<feature-name>.md`; use kebab-case (e.g., `chunked-tilemap.md`), no spaces, concise names.
- Use the [review process](../feature-doc-review/SKILL.md) to check the document for issues
