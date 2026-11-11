# D-003: License GPL-3.0-or-later for code and logo

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** Public open-source release planned; logo must be own work (original logo.jpg is third-party, never used).
- **Decision:** Code and the newly drawn SVG logo are licensed GPL-3.0-or-later. Logo is drawn from scratch: generic high-speed train in ICE style, no DB logo, no red DB stripe, no "ICE" lettering, own-drawn poop (no emoji set artwork).
- **Why:** Developer wants forks to stay open source; one license for everything keeps it simple.
- **Alternatives considered:** MIT (rejected by developer); CC BY-SA for logo (rejected).
- **Consequences:** LICENSE file with GPL-3.0 text; dependencies must be GPL-compatible (Avalonia MIT, CommunityToolkit.Mvvm MIT, xUnit Apache-2.0: ok). No third-party assets in repo.
