# Feature Title

## Overview

Detailed description of the feature including purpose and intent as well as intended use cases

## Table of contents

- Table of contents with links to child sections, including subsections and child documents (which will be in a folder matching the feature document name).

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

Detailed table (sorted alphabetically by the term) of terms not considered 'common english'. Include references to articles about the term.

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

### Feature requirements

Detailed description of the feature and its requirements

A list of feature-level requirements in Given/When/Then syntax.

If a feature is considered complete then indicate with a ***COMPLETE*** prefix.

```markdown
- (***requirement status***) Requirement descriptive text
  - GIVEN a precondition
  - WHEN an event happens
  - THEN an action is taken / a result property is true

> Details of missing implementation / link to phase with missing implementation

- (***requirement status***) Requirement descriptive text
  - GIVEN a precondition
  - WHEN an event happens
  - THEN an action is taken / a result property is true

> Details of missing implementation / link to phase with missing implementation
```

### Phase N

***Phase status***

#### Objective

Paragraph description of objective.

Description of success criteria

#### Technical details

Detailed description of how this part of the feature works.

Include technical details, diagrams, and deeper explanation of what this element tries to achieve.

#### Phase requirements

A list of phase-level requirements in Given/When/Then syntax.

If a phase is considered complete then indicate with a ***COMPLETE*** prefix.

```markdown
- (***requirement status***) Requirement descriptive text
  - GIVEN a precondition
  - WHEN an event happens
  - THEN an action is taken / a result property is true

> Details of missing implementation / link to phase with missing implementation

- (***requirement status***) Requirement descriptive text
  - GIVEN a precondition
  - WHEN an event happens
  - THEN an action is taken / a result property is true

> Details of missing implementation / link to phase with missing implementation
```

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
