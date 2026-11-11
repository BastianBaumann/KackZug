# Known Issues & Workarounds (no secrets, no raw logs; one or two lines each)

> 🧠 **MEMORY: read before acting · append, don't overwrite · this file (not the chat) is the source of truth · protocol: AgentStarter (see AgentHub/STATE.md → Protocol).**

| Since | Issue | Workaround | Task |
|---|---|---|---|

- [2026-10-21] Play Protect shows "Harmful app blocked" for KackZug APKs since v0.4.0-dev.1 (first build with the in-app updater, REQUEST_INSTALL_PACKAGES), on manual install and on the in-app update. Workaround: More details -> Install anyway. Fix options (developer account needed): Play Protect appeal for the APK, Android developer verification of the app.
- [2026-10-21] Confirmed by the developer: v0.3.1 (no updater) installed without the Play Protect warning, so the trigger is the updater build (install permission). Every way for an app to install an APK itself needs REQUEST_INSTALL_PACKAGES on Android 8+, so it cannot be avoided in code. Appeal: Play Protect warning dev guidance https://developers.google.com/android/play-protect/warning-dev-guidance ("File a Play Protect appeal" on https://support.google.com/googleplay/android-developer/answer/2477981); each new version may have to be submitted again.
