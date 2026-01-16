---
name: feature-doc-elaborate
description: Produce a feature/component documentation that adheres to repo standards.
---

# Feature Elaboration

## Purpose of this skill

The purpose of this skill is to generate a document that describes how a feature might be implemented, with enough context and explanatory content that a developer with limited domain knowledge would be able to progress with feature implementation.

## When to use this skill

Use this skill when the user wants to describe a new feature.

## Process inputs

- A description of the requested feature
- Optionally, a document partially describing the feature

## Process outputs

- A detailed document describing the feature including architectural considerations and an implementation guide

## How to generate the new feature document

- Create or update a Markdown Document under [docs](../../docs/features) using the [provided template](../../../docs/templates/feature-doc-template.md).
- When adding code snippets:
  - Include a short paragraph description before the code snippet explaining what it does.
  - Remember to ensure any custom types have been defined before trying to use them
- Link from [docs/README.md](../docs/README.md) and validate links.

- Inputs: feature name and description, config keys.
- Outputs: new/updated doc path; docs index entry.
- Preconditions: gather architecture/context; decide doc location.
- Verification: link check passes; content follows principles; examples compile.
- Rollback: revert doc if requirements unmet; fix and re-run.
- Naming: place feature docs at `docs/features/<feature-name>.md`; use kebab-case (e.g., `chunked-tilemap.md`), no spaces, concise names.
- Use the [review process](../feature-doc-review/SKILL.md) to check the document for issues and fix them
