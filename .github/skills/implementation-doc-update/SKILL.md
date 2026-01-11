---
name: implement-doc-update
description: Perform changes to implement a feature
---

# Update Documentation

## Purpose of this skill

The purpose of this skill is to generate a meaningful and useful set of documentation that explains how an implementation works.

It is not to replicate API documentation (which drives the developer experience through XML comments), but rather to complement it with context and explanatory information, such that a developer with limited domain knowledge would be able to work on this project.

Compared to the [feature-doc-elaborate skill](../feature-doc-elaborate/SKILL.md), which is focused on the ***plan*** for an implementation, this skill is focused on documenting an implementation that either ***already exists*** or is ***currently in development***.

## When to use this skill

Use this skill when the user wants the agent to update documentation based on an implementation, or to update documentation while performing an implementation.

## Process inputs

- A workspace containing an implementation or partial implementation
- Optionally, a focus or area to update in particular
- Optionally, a partially created or outdated set of documentation
- Optionally, a completed and up to date set of documentation

## Process outputs

- A completed and up to date set of documentation

## How to update documentation

- Read and analyse the code in the repository
- Create a model of the implementation organised by functional area and component
- Create or update a Markdown Document under [docs](../../docs/)
- For ea
- Link from [docs/README.md](../docs/README.md)
- Validate links

### Principles of documentation

- Use common 'plain English'
- Words that are not 'plain English', such as domain terms
