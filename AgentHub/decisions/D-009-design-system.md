# D-009: Design source is the developer's Design System artifact "ZugKacke"

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** Developer: "fuer Logos etc gibts ein DesignSystem ... das generell zum Designen genutzt wird". Artifact https://claude.ai/artifact/7Uq5gAxSNUieFgi412nJaK (title "ZugKacke", type Design System): tokens (light/dark), Inter font, 16 outline icons, 4 logos (app icon on #EC0016, wordmark "ZugKacke", mark, notification small icon), 13 component previews incl. AlarmScreen (full-screen alarm), README brand book in German.
- **Decision:** All visual work (logo, icons, colours, type, components, texts tone) is taken from this design system. Read project/README.md + tokens.json first. Own drawings only where it has nothing, and say so. Supersedes the logo part of D-003 once Q-006/Q-007 are answered (licence stays GPL-3.0-or-later; design system assets are the developer's own work).
- **Consequences:** Open-source check needed: README of the design system leans on the DB Navigator look and uses DB red #EC0016 as brand colour (conflicts with D-003 "no red DB stripe" intent) -> Q-006. Wordmark says "ZugKacke", app is "KackZug" -> Q-007.
- **Update 2026-10-20:** Q-006 = real orange instead of DB red (icon background and brand colour), Q-007 = app stays KackZug, wordmark adapted.
