---
description: 'Create focused tests for service logic and MVC controller behavior with readable demo-friendly coverage.'
name: 'mvc-testing-services-controllers'
agent: 'agent'
argument-hint: 'Feature under test and expected behaviors (happy path, validation failures, duplicates)'
---

# MVC Testing for Services and Controllers

## Mission
Generate and maintain tests that validate service business behavior and controller response behavior without overcomplicating the test suite.

## Scope and Preconditions
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Follow C# testing instruction file conventions already present in repository.
- Keep tests deterministic and easy to explain.

## Inputs
- Feature under test: ${input:featureUnderTest:Contact submission flow}
- Service contract: ${input:serviceContract:IContactService}
- Controller action(s): ${input:controllerActions:GET Contact, POST Contact}
- Test framework in repo: ${input:testFramework:MSTest|NUnit|xUnit}

## Workflow
1. Identify service rules and define core unit test cases.
2. Identify controller behaviors and define response-oriented tests:
   - invalid model returns same view
   - valid submission redirects (PRG)
   - duplicate submission handling path
3. Keep controller tests focused on orchestration, not service internals.
4. Use clear names and concise setup.
5. Add missing edge-case tests for null/empty/format boundaries as needed.

## Output Expectations
- Provide test file edits grouped by service tests and controller tests.
- Include a short coverage map of scenarios addressed.
- Note any untestable paths and why.

## Quality Assurance
- Service rules are covered by unit tests.
- Controller behavior paths are covered.
- Duplicate submission behavior is tested.
- Test names and assertions are readable for demo explanation.
