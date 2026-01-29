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

## See also

For large features that are split across multiple files, a list of links to those files.

These will be in a well-ordered file structure:

- `docs/features/<this-feature>.md` - this file
- `docs/features/<this-feature>/` - folder containing child features
  - `docs/features/<this-feature>/<child-feature>.md` - child features documentation

This section will contain a table of contents with links to those child features

This structure is repeated as deep as is needed.

This section will also contain links to related/sibling features by topic and parent features by structure.

## References

A list of links to external resources such as library documentation or articles about
relevant techniques
