---
name: implementation-doc-review
description: Review functional area and component documentation to check that it adheres to repo standards.
---

# Feature Review

## Prerquisites

- markdownlint is installed and can be called via `npx markdownlint-cli`

## When to use this skill

Use this skill when the user wants to check whether documentation fits required criteria.

## Process inputs

- An existing document
- Optionally, a focus to review, such as a particular section or aspect

## Process outputs

- A report on the document's adherence to guidelines
- Suggested edits to make a document more compliant

## Commands

- Run the command `npx markdownlint-cli **/docs/*.md` to check for and report markdown errors
- Run the script [check-doc-links.ps1](../../../scripts/check-doc-links.ps1) to check for bad links

## How to review the feature document

- Read the provided document
- Check for stylistic consistency, grammar, plain English and legibility
- Check for relevance to the subject matter
- Check for correctness compared to current implementation / goals
- Check for adherence to the checklist
- Check for adherence to the template
- Run the command `npx markdownlint-cli **/docs/*.md` to check for and report markdown errors
- Run the script [check-doc-links.ps1](../../../scripts/check-doc-links.ps1) to check for bad links
