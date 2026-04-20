---
description: 'Review and refactor MVC code for thin controllers, clean naming, and architecture compliance.'
name: 'mvc-review-refactor'
agent: 'agent'
argument-hint: 'Scope to review (for example: Controllers/HomeController.cs and related service/view model files)'
---

# MVC Review and Refactor

## Mission
Review selected project files and produce targeted refactors that improve architecture compliance, readability, and maintainability.

## Scope and Preconditions
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Prioritize behavior preservation and low-risk edits.
- If uncertain about behavior, state assumptions before changing logic.

## Inputs
- Review scope: ${input:reviewScope:controllers-services-viewmodels}
- Refactor intensity: ${input:refactorIntensity:conservative}
- Focus areas: ${input:focusAreas:thin controllers, naming, separation of concerns}

## Workflow
1. Inspect current code and identify architecture drift:
   - business logic in controllers
   - mixed entity/view model usage
   - unclear naming or oversized methods
2. Rank findings by severity and impact.
3. Apply conservative refactors that keep behavior intact.
4. Improve naming consistency and method responsibility boundaries.
5. Summarize remaining risks and suggested follow-ups.

## Output Expectations
- Present findings first, ordered by severity with file references.
- Provide exact file edits for refactors.
- Include a compact before/after architecture note.

## Quality Assurance
- Controllers are thinner after refactor.
- Naming is clearer and consistent.
- Architectural boundaries are more explicit.
- No intentional behavior regression introduced.
