---
description: 'Build a clean centered Contact Razor UI with minimalist academic style and responsive layout.'
name: 'mvc-razor-contact-ui'
agent: 'agent'
argument-hint: 'Target Razor view and any style constraints (for example: use site.css only)'
---

# MVC Razor Contact UI

## Mission
Create or refine the Contact page UI to match a clean, centered, professional student-project concept.

## Scope and Preconditions
- Follow repository guidance in `../instructions/copilot-instructions.md`.
- Use semantic Razor/HTML and simple maintainable CSS.
- Avoid heavy effects and unnecessary components.

## Inputs
- Target view: ${input:targetView:Views/Home/Contact.cshtml}
- Styling location: ${input:styleLocation:wwwroot/css/site.css}
- Navbar project title: ${input:projectTitle:DemoApp}

## Workflow
1. Ensure top navigation includes project name on the left and Contact link.
2. Build centered page container on a light background.
3. Add large centered heading: Contact Us.
4. Render four stacked fields in this order: Name, Email, Phone, Message.
5. Add full-width blue Submit button with clear hover/focus states.
6. Keep spacing, typography, and alignment consistent across desktop/mobile.
7. Ensure markup remains accessible and easy to explain.

## Output Expectations
- Provide complete Razor markup changes and CSS changes.
- Explain key UI choices in concise, demo-friendly language.
- Note responsive behavior for small screens.

## Quality Assurance
- Layout is centered and minimalist.
- Required visual elements are present.
- Submit button is clearly visible and full width.
- View remains clean and readable.
