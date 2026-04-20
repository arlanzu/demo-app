---
description: 'Implement a complete MVC feature flow: controller action, service method, view model, and Razor wiring.'
name: 'mvc-feature-flow'
agent: 'agent'
argument-hint: 'Feature name and user flow (for example: Contact submit with success page)'
---

# MVC Feature Flow

## Mission
Implement one end-to-end MVC feature with clear layering: controller delegates to service, service executes use case, view model drives Razor view.

## Scope and Preconditions
- Use existing architecture and naming conventions.
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Keep code readable and demo-friendly.
- Ask for missing flow details before generating code.

## Inputs
- Feature name: ${input:featureName:Contact}
- User flow summary: ${input:userFlow:Get form, submit form, show success}
- Domain object involved: ${input:domainObject:ContactSubmission}
- Success redirect/action: ${input:successTarget:ThankYou}

## Workflow
1. Define or align feature-specific view model(s) for input/output.
2. Define or align service contract and implementation for the use case.
3. Add thin controller actions that only:
   - validate input
   - call service
   - return view/redirect
4. Wire Razor views to the view model with clear validation message locations.
5. Apply PRG flow on successful POST.
6. Ensure async calls for I/O and persistence paths.

## Output Expectations
- Provide complete file edits for controller, service, view model, and relevant view(s).
- Summarize how responsibilities are split across layers.
- List assumptions and any follow-up tasks.

## Quality Assurance
- Controller contains no business logic.
- Service contains the feature business flow.
- View model is not the domain entity.
- POST success path uses redirect.
