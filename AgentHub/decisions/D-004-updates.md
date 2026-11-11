# D-004: Distribution via GitHub Releases, in-app update hint only

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** Public repo, no Play Store. Developer wants users to find updates; updates must install over the old version without uninstalling.
- **Decision:** GitHub Actions builds, signs and publishes an APK to a GitHub Release on version tag. App checks GitHub releases/latest (unauthenticated) and shows a hint "neue Version verfuegbar" that opens the release page in the browser. No in-app download/install, no REQUEST_INSTALL_PACKAGES. README mentions Obtainium as optional auto-updater.
- **Why:** Minimal code and permissions; browser download of the APK installs as an update.
- **Alternatives considered:** full in-app installer (more code + permission); Obtainium only (users without it never hear of updates); Actions artifacts (need login, expire).
- **Consequences:** Fixed package id, one release keystore forever (developer creates it, stored as GitHub secret + offline backup), monotonically increasing versionCode set by CI. Update check only works once repo is public. Local debug builds use a different key (developer must uninstall once when switching).
