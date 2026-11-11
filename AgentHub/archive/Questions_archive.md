# Questions archive

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

## Round 1 (2026-10-20)
### Q-001: Datenquelle fuer Import und Live-Pruefung · status: answered · priority: high · blocks: design
- **Question:** Offizielle Timetables-API braucht einen Schluessel; im oeffentlichen APK waere er auslesbar. Welche Quelle?
- **Options:** A) inoffizielle bahn.de/Vendo-Schnittstelle direkt vom Handy (kein Schluessel, gleiche Daten wie DB Navigator, vbid-Aufloesung braucht sie ohnehin; Risiko: Aenderungen/Sperren, rechtlich grau) · B) offizielle Timetables-API, jeder Nutzer traegt eigenen Schluessel ein (sauber, CC BY 4.0, aber Registrierung im DB API Marketplace pro Nutzer) · C) A als Standard, B optional spaeter · D) Transitous (nur offene Daten, DB-Echtzeit unklar)
- **Recommendation:** C (A jetzt, hinter Interface; B als spaetere Option)
- **Answer:** 2026-10-20, (@Dev/Bastian): C -> D-002
### Q-002: Lizenz des Codes · status: answered · priority: med · blocks: repo setup
- **Options:** A) MIT · B) GPL-3.0-or-later
- **Recommendation:** A (einfach, alle Abhaengigkeiten MIT/Apache)
- **Answer:** 2026-10-20, (@Dev/Bastian): GPL-3.0-or-later -> D-003
### Q-003: Logo-Lizenz und Stil · status: answered · priority: med · blocks: logo task
- **Options:** Logo selbst als SVG gezeichnet (generischer Hochgeschwindigkeitszug im ICE-Stil ohne DB-Logo, ohne Schriftzug "ICE", ohne roten DB-Streifen; Haufen ebenfalls selbst gezeichnet, kein Emoji-Set), Lizenz A) wie Code · B) CC BY-SA 4.0
- **Recommendation:** A
- **Answer:** 2026-10-20, (@Dev/Bastian): A (logo under same license as code, GPL-3.0-or-later) -> D-003
### Q-005: PLAN.md im Repo-Root · status: answered · priority: low
- **Question:** PLAN.md ist ein KI-Plan; mit K1 = invisible gehoert er nicht ins Repo. Kopie liegt in AgentHub/Vision.md.
- **Options:** A) lokal lassen, nie committen (aktuell ausgeschlossen) · B) loeschen
- **Recommendation:** A
- **Answer:** 2026-10-20, (@Dev/Bastian): A, PLAN.md stays local and excluded
### Q-004: Updates und Verteilung · status: answered · priority: med · blocks: CI task
- **Options:** A) GitHub Actions baut bei Tag ein signiertes Release, App prueft releases/latest und bietet Installation an · B) nur Releases, Update-Hinweis ueber Obtainium (kein eigener Code)
- **Recommendation:** A (plus Hinweis auf Obtainium im README)
- **Answer:** 2026-10-20, (@Dev/Bastian): hint only, links to latest release page; must update in place -> D-004

## Round 2 (2026-10-20): Design System
### Q-006: Logo colour orange vs DB red · status: answered · priority: high · blocks: T-023
- **Question:** Developer wants the logo "mit orangenem Hintergrund". The only logo with a background (zugkacke-app-icon.svg) uses #EC0016, which is DB red (DB brand colour), and the design system copies the DB Navigator look. For a public open-source app that leans on DB trade dress.
- **Options:** A) real orange background (e.g. #F26B1D), rest of the icon unchanged; B) keep #EC0016 as in the design system; C) other colour from the developer
- **Recommendation:** A (matches "orange", avoids DB red); same question for the brand colour in tokens
- **Answer:** 2026-10-20, (@Dev/Bastian): A, real orange instead of DB red (icon background and brand colour) -> T-023
### Q-007: App name in the wordmark · status: answered · priority: med · blocks: T-023
- **Question:** Design system and wordmark say "ZugKacke"; the app, repo and package are "KackZug".
- **Options:** A) app stays KackZug, wordmark adapted ("Kack" + "Zug") · B) rename app to ZugKacke (repo, package id must then change before first public release)
- **Recommendation:** A
- **Answer:** 2026-10-20, (@Dev/Bastian): A, app stays KackZug, wordmark adapted -> T-023

### Q-009: Open the exact connection in DB Navigator · status: answered · priority: low · blocks: none
- **Question:** The Navigator ignores bahn.de search links and has no documented deep link. Only its own share links (`bahn.de/buchung/start?vbid=...`) might open inside the app. Does tapping such a shared link (e.g. sent to yourself in a messenger) open the connection in the Navigator?
- **Options:** A) keep "start page" (now); B) if the link opens in the app: store the share link at import and open it (shows the connection of the day it was shared, works only for connections shared after the update)
- **Recommendation:** test first; B only if the app takes the link
- [2026-10-21] (@Dev/Bastian) NOTE: still only the start page with v0.2.2-dev.4 (expected); test of a shared link not done yet. Option C noted: create a fresh share link for the travel day via bahn.de (search + share endpoint, unverified) and open it in the Navigator
- **Answer:** [2026-10-21] developer: a shared link opens the connection in DB Navigator -> option C (fresh share link for the travel day) -> T-042

### Q-010: Update check needs a public repository · status: answered 2026-10-21 (A, repo public) · priority: medium · blocks: real test of T-019/T-041
- **Question:** GitHub answers 404 for releases of the private repo, so the app can never find updates. Make the repo public, or keep the check and accept "konnte nicht nachsehen"?
- **Options:** A) make the repo public (README already written for it); B) keep private, check stays without result
- **Recommendation:** A when you are ready to publish
- **Answer:**
