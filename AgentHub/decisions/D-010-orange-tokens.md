# D-010: Orange brand tokens instead of DB red

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian via Q-006; values chosen by @Core)
- **Decision:** App icon background #F26B1D (no text on it). brand #C84B0C in both themes (white text 4.7:1), brand-pressed #A63E09, brand-ink #B93D0A light (5.1:1 on bg) / #FF8A3D dark (7.1:1), brand-soft #FDEBDD / #3A2012. All other tokens (surfaces, ink, status ok/late/cancel/unknown, alarm, shift-1..6) taken unchanged from the design system tokens.json. User accent override still possible (setting), default = orange; legacy teal #1E9E8B treated as default. Secondary colour picker removed (design system has no gradients).
- **Why:** WCAG 4.5:1 for button text; avoid DB brand colour for a public app.
- **Consequences:** Design system artifact itself still shows DB red; developer may update it there.
