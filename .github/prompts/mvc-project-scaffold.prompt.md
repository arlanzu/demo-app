---
description: 'Scaffold a clean ASP.NET Core MVC + EF Core SQLite project structure for DemoApp.'
name: 'mvc-project-scaffold'
agent: 'agent'
argument-hint: 'Feature scope and folder strategy (for example: contact-form-only, standard layered structure)'
---

# MVC Project Scaffold

## Mission
Create or align the repository structure for an ASP.NET Core MVC project using C#, SQLite, and Entity Framework Core with thin controllers, service layer, and separate domain entities/view models.

## Scope and Preconditions
- Work only within ASP.NET Core MVC patterns.
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Keep the result easy to explain in a university demo.
- If required context is missing, ask for it before editing files.

## Inputs
- Scope: ${input:scope:contact-form-only|full-foundation}
- Folder strategy: ${input:folderStrategy:Domain-Application-Infrastructure-Presentation}
- Data strategy: ${input:dataStrategy:single-DbContext-with-SQLite}

## Workflow
1. Analyze current repository structure and identify missing architecture folders, interfaces, and configuration points.
2. Propose minimal folder and naming alignment for:
   - Domain entities
   - Application services and interfaces
   - Infrastructure data access and DbContext
   - Presentation controllers, view models, and views
3. Configure or align SQLite + EF Core registration in startup and appsettings.
4. Ensure dependency injection resolves service abstractions instead of concrete controller dependencies.
5. Keep controllers free of business logic and persistence logic.
6. Document any assumptions and list unresolved inputs.

## Output Expectations
- Create or update only files required for the scaffold.
- Provide a concise changed-files summary grouped by architecture layer.
- Include a short rationale for each structural decision.

## Quality Assurance
- Verify MVC-only scope (no Minimal APIs, no Razor Pages, no Blazor).
- Verify controllers are thin and services own business flow.
- Verify entities and view models are separate types.
- Verify SQLite connection and DbContext registration are present.
