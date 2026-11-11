# D-012: In-app updater as a reusable source module

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-21 · **Status:** accepted (2026-10-21, by @Dev/Bastian; details designed by @Core)
- **Decision:** The app updates itself: it reads GitHub Releases in the background, downloads the APK itself, verifies it and starts the Android installer. No browser, no GitHub page. The update source is an interface, so another feed (own server) can be added later. The updater is built as a .NET source module (core + Android installer) and kept in the AgentStarter repo under modules/app-updater, copied into or referenced by other apps. Work happens on its own branch (feature/updater) until the developer approves.
- **Why:** developer request 2026-10-21; reuse in other apps without a package feed.
- **Consequences:** Android always asks the user to confirm an install the first time (REQUEST_INSTALL_PACKAGES + "unknown sources" for KackZug); release assets need a checksum; the existing update banner/check (T-019, T-041) is replaced by the module.
