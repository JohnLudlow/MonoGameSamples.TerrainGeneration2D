# Feature Title

## Overview

Detailed description of the topic (functional area or component) including purpose and intent as well as intended use cases.

## Table of contents

- Table of contents with links to child sections

## Definition of terms

Detailed list of terms not considered 'common english'. Include references to articles about the term

| Term | Meaning | Reference |
| ---- | ------- | --------- |
|      |         |           |
|      |         |           |

## Technical guide

Detailed model of the technical details of the component or functional area, including logical flow and flow of data within the system.

- Include constraints such as performance considerations and related components
- Include ASCII-art or mermaid diagrams where appropriate
- Include KaTeX math where relevant, with plain-English explanations

### Troubleshooting

- Details of common problems and errors with this component, with causes and resolution steps

### Examples

- Example of how this component can be configured and used
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

## Navigation

- Links to related pages, related by topic
- Links to related pages, related by structure (for example, parent or child topics)
