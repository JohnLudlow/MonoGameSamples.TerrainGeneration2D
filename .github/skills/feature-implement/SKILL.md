---
name: feature-implement
description: Perform changes to implement a feature
---

# Feature Implementation

## When to use this skill

Use this skill when the user wants the agent to implement a new feature based on a design.

## Process inputs

- A detailed design document passing review
- Optionally, a partially completed implementation

## Process outputs

- A completed implementation with high test coverage
- If appropriate, unit tests and benchmarks covering the feature
- If appropriate, documentation changes covering the feature

## How to implement the feature

- Read the provided feature document and review it for correctness and completeness
- Implement the feature step-by-step until complete

### Principles of development

- Respect .editorconfig and other settings
- Prefer var
- Prefer modern C# features
- Prefer fail-fast over carry-on-in-hope
- Minimise warnings and minimise unknown behaviour
  - For example, if a possibly-null reference is passed, prefer to add a null check
- Prefer LINQ over rolling-your-own queries and transformations
- Provide a detailed XML comment on all members, describing purpose
- Where these rules prove subobtimal (for example, using LINQ would be slow),
  provide a comment above the usage explaining why
