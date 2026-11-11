# D-011: App and README in English and German

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-21 · **Status:** accepted (2026-10-21, by @Dev/Bastian)
- **Decision:** The app UI is available in English and German. Setting "Sprache / Language": System (default), Deutsch, English; system = German when the device language is German, otherwise English. Notifications and alarm texts follow the same setting. README.md is English (public repo), README.de.md is the German copy; both link to each other at the top. Code, identifiers and commits stay English.
- **Why:** developer request 2026-10-21; the repo is public.
- **Consequences:** supersedes the "Deutsch only" part of D-006. Every user-facing text goes through one string table; new texts need both languages; the README needs to be kept in sync in both files.
