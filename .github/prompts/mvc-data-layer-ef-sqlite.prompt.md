---
description: 'Create or update data layer artifacts: entity model, DbContext mapping, and EF Core migration steps for SQLite.'
name: 'mvc-data-layer-ef-sqlite'
agent: 'agent'
argument-hint: 'Entity and persistence requirements (fields, constraints, indexing needs)'
---

# MVC Data Layer with EF Core SQLite

## Mission
Design and implement persistence changes using EF Core and SQLite with clear, maintainable mappings and migration guidance.

## Scope and Preconditions
- Keep persistence logic in infrastructure/data layer.
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Keep model shape aligned with service and view model needs.

## Inputs
- Entity name: ${input:entityName:ContactSubmission}
- Fields and types: ${input:fields:Name string, Email string, Phone string, Message string, CreatedAt DateTime}
- Constraints: ${input:constraints:required fields, max lengths, reasonable defaults}
- Migration name: ${input:migrationName:AddContactSubmission}

## Workflow
1. Create or update domain entity and persistence mapping as needed.
2. Update DbContext with DbSet and fluent/data-annotation mapping.
3. Apply explicit constraints (required, max lengths, unique/index where applicable).
4. Ensure SQLite-compatible types and defaults.
5. Provide migration creation and database update command sequence.
6. Explain why each persistence decision was made.

## Output Expectations
- Include concrete file edits for entity and DbContext.
- Include migration commands and expected result checks.
- Highlight backward-compatibility or data-impact considerations.

## Quality Assurance
- Entity mapping is explicit and readable.
- DbContext is consistent with architecture boundaries.
- Migration steps are complete and executable.
- Constraints align with validation/security requirements.
