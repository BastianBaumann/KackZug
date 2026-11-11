# D-006: Developer preferences, German only, medium test depth

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Decision:** knowledge/developer-preferences.md applies fully (code style, UI taste incl. two adjustable colors, light/dark). App UI and README German only; code, identifiers, commits English; AgentHub docs German/English mixed ok. Tests: medium: xUnit for core logic (share-link parsing, API response mapping, matching/status, check planner incl. DST, week rollover, storage roundtrip) with small hand-made fixtures; UI via functional checks; tests run in GitHub Actions on push.
- **Why:** Target users are German commuters; developer wants moderate tests.
- **Alternatives considered:** bilingual README/UI (rejected), functional checks only.
