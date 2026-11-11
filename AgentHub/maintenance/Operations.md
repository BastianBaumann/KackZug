# Operations (build, release, rollback)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

## CI
- `.github/workflows/ci.yml`: on push to main/dev and PRs: core tests + desktop build; Android workload + Release APK build (unsigned check).

## Release
- `.github/workflows/release.yml`: on tag `v*`: tests, version = tag without v, versionCode = minutes since 2026-01-01 UTC, signed `dotnet publish` (Release, all ABIs, ~43 MB), uploads `KackZug-<version>.apk` to a GitHub release with generated notes.
- Secrets (names only): ANDROID_KEYSTORE_BASE64, ANDROID_KEYSTORE_PASSWORD, ANDROID_KEY_ALIAS, ANDROID_KEY_PASSWORD. Owner: developer (T-018). The keystore must never change, otherwise updates do not install over existing apps.
- Process: merge dev -> main (developer OK), tag `vX.Y.Z` on main, push tag (developer OK; agent K2 covers branch pushes only).
- App update check reads releases/latest (works only when the repo is public).

## Keystore creation (developer, once)
- keytool: `C:/Program Files (x86)/Android/openjdk/jdk-17.0.14/bin/keytool.exe -genkeypair -v -keystore kackzug-release.keystore -alias kackzug -keyalg RSA -keysize 4096 -validity 36500`
- Base64 for the secret (PowerShell): `[Convert]::ToBase64String([IO.File]::ReadAllBytes("kackzug-release.keystore")) | Set-Clipboard`
- Back up keystore + passwords offline (password manager). Never put it into the repo (.gitignore covers *.keystore).

## Rollback
- Delete the broken release on GitHub and publish a new higher version (versionCode always increases; downgrades do not install over newer builds).
