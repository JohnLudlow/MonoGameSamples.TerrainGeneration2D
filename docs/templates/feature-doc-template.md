# Feature Title

## Overview

Detailed description of the feature including purpose and intent as well as intended use cases

## Feature requirements

Detailed description of the feature and its requirements

## Feature status

An indication of current status.

- Not started
- In discovery
- In design
- In development
- In test
- In review
- Completed
- Abandoned
- Blocked

May also include additional information such as reason for the feature being blocked.

If the feature is composed of multiple parts and some are implemented, list the components and their status.

## Definition of terms

Detailed list of terms not considered 'common english'. Include references to articles about the term

| Term | Meaning | Reference |
| ---- | ------- | --------- |
|      |         |           |
|      |         |           |

## Architectural considerations and constraints

Detailed model of the feature's architectural consideration and flow of data within the system.

- Include constraints such as performance considerations or affected components
- Include ASCII-art or mermaid diagrams where appropriate
- Include KaTeX math where relevant, with plain-English explanations

## Implementation guide

Detailed step-by-step implementation guide, including code snippets, unit tests and benchmarks.

If requested, write steps to follow Test Driven Development principles, leading with a minimal breaking
test, follwed by a minimal change to fix the test, followed by refactor, repeating until the feature is
complete.

If the feature is extensive and complex, create multiple subsections to make these steps more legible.

### Phase N

#### Objective

Paragraph description of objective.

Description of success criteria

#### Technical details

Detailed description of how this part of the feature works.

Include technical details, diagrams, and deeper explanation of what this element tries to achieve.

#### Examples

- Minimal, compile-ready examples with XML docs.
- Include the relevant API and usage snippets.

```csharp
// path/to/file.cs
// parent class or method name

/// <summary>
/// Selects the next cell using Shannon entropy when enabled.
/// </summary>
/// <remarks>
/// Avoid allocations; called in hot path. See WfcTimeBudgetMs.
/// </remarks>
public int SelectNextCell(/* params */)
{
  // example body
  return 0;
}
```
