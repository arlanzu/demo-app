---
description: 'Implement server-side validation and secure form handling with anti-forgery and duplicate submission protection.'
name: 'mvc-validation-security'
agent: 'agent'
argument-hint: 'Form endpoint and fields to secure (for example: Contact POST Name Email Phone Message)'
---

# MVC Validation and Security

## Mission
Harden an MVC form workflow with server-side validation, anti-forgery protection, and duplicate submission protection while preserving user-friendly behavior.

## Scope and Preconditions
- Follow repository guidance in `../instructions/copilot-instructions.md` and `../instructions/security-and-owasp.instructions.md`.
- Keep implementation simple and explainable.
- Stop and ask for clarification if duplicate-handling policy is undefined.

## Inputs
- Form endpoint: ${input:formEndpoint:Home/Contact POST}
- View model: ${input:viewModel:ContactFormViewModel}
- Duplicate policy: ${input:duplicatePolicy:ignore-repeat-and-show-friendly-message}

## Workflow
1. Add or refine server-side validation rules for all input fields.
2. Ensure anti-forgery token generation and validation are applied for POST handling.
3. Implement duplicate submission strategy:
   - PRG pattern
   - server-side idempotency token check
   - clear user feedback when duplicate is detected
4. Ensure controller returns invalid ModelState to view with helpful messages.
5. Ensure validation does not rely only on client-side scripts.
6. Summarize security controls and rationale.

## Output Expectations
- Provide file edits for view model, controller/action, and Razor form integration.
- Include a short threat/abuse checklist specific to this flow.
- Document how duplicate prevention behaves under refresh/retry.

## Quality Assurance
- Required fields and formats are enforced server-side.
- Anti-forgery is active for POST.
- Duplicate submissions are blocked or safely ignored.
- User receives clear and non-technical feedback.
