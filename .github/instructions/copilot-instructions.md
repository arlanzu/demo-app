# DemoApp Copilot Instructions

## Purpose
Use these instructions for all Copilot suggestions in this repository.

This is a university-style ASP.NET Core MVC project in C# that demonstrates clean architecture basics with a simple Contact form flow backed by SQLite and Entity Framework Core.

## Tech Stack and Scope
- Framework: ASP.NET Core MVC only (no Razor Pages, no Blazor, no Minimal API)
- Language: C#
- Data: SQLite + Entity Framework Core
- Views: Razor (.cshtml)

## Primary Goal
Keep code easy to explain in a university demo:
- predictable structure
- simple and readable naming
- thin controllers
- business logic in services
- explicit validation and security protections

## Required Architecture
Organize code with clear separation of concerns.

- Domain layer: entities and domain rules only
- Application layer: service interfaces and implementations
- Presentation layer: MVC controllers + view models + Razor views
- Infrastructure layer: EF Core DbContext and data access implementation

Required conventions:
- Keep controllers thin: map input, call services, return view/redirect
- Do not put business rules or data access logic inside controllers
- Keep entities separate from view models
- Use constructor dependency injection
- Use async methods for I/O calls (EF Core and service methods)

## MVC Conventions
When generating MVC code:
- Create dedicated view models for each form/page scenario
- Use explicit model binding and server-side validation
- Return the same view with validation errors when ModelState is invalid
- Use PRG (Post-Redirect-Get) for successful form submissions
- Keep actions focused and short

## Data and EF Core (SQLite)
When generating data access:
- Use EF Core with a DbContext configured for SQLite
- Keep DbContext configuration in startup/Program setup and appsettings
- Use migrations for schema changes
- Keep entities simple and persistence-friendly
- Prefer clear, straightforward queries over complex abstractions

## Validation and Security Requirements
All generated form handling must include:
- DataAnnotations (or equivalent) for server-side validation
- Anti-forgery protection on form posts
- Validation summary and per-field validation messages in Razor views
- Input length limits and appropriate field formats (for Email/Phone)

Do not rely only on client-side validation.

## Duplicate Submission Protection
Implement duplicate submission protection for contact form flows.

Preferred approach:
- Use PRG pattern after successful POST
- Add a submission token/idempotency mechanism stored and validated server-side
- Reject or safely ignore repeated submissions with the same token
- Show user-friendly feedback when a duplicate is detected

## UI and Razor View Requirements
Match this exact visual direction for the Contact page:
- simple, clean, centered contact form page
- light background
- top navigation bar with project name on the left and links including Contact
- large centered heading: Contact Us
- four vertically stacked fields: Name, Email, Phone, Message
- wide blue submit button labeled Submit
- minimalist, professional academic/student-project style

Razor and front-end rules:
- Use clean semantic HTML
- Keep CSS readable and minimal
- Avoid heavy visual effects or complex animations
- Keep spacing consistent and centered layout responsive on desktop/mobile
- Keep views tidy and easy to present in a demo

## Naming and Readability Rules
- Use clear, descriptive names in English
- Follow C# conventions (PascalCase for types/methods, camelCase for locals/fields)
- Keep methods short and purpose-specific
- Add brief comments only where intent is not obvious
- Prefer straightforward code over clever patterns

## What Copilot Should Prioritize
When multiple valid options exist, prefer the one that is:
1. easiest to explain to students
2. aligned with thin-controller + service-layer architecture
3. secure by default (anti-forgery, validation, safe form handling)
4. consistent with MVC + EF Core SQLite standards

## What to Avoid
- Minimal API endpoints in this project
- Business logic inside controllers
- Returning entities directly to views
- Mixing persistence models with view models
- Skipping anti-forgery tokens in POST forms
- Skipping duplicate submission handling for contact POST actions
- Overengineered patterns that reduce clarity for a demo

## Existing Instruction Files
Also follow these repository instruction files when relevant:
- .github/instructions/csharp.instructions.md
- .github/instructions/aspnet-rest-apis.instructions.md
- .github/instructions/dotnet-architecture-good-practices.instructions.md
- .github/instructions/security-and-owasp.instructions.md
- .github/instructions/ai-prompt-engineering-safety-best-practices.instructions.md

If guidance conflicts, prioritize this file for repository-specific implementation choices in this MVC project.