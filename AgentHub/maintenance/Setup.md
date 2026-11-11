# Setup (prereqs, build, run, test)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- Prereqs: .NET 10 SDK + `android` workload, Android SDK with platform 36 in `%LOCALAPPDATA%\Android\Sdk`, JDK 17.
- Build all: `dotnet build KackZug.slnx` (first Android build ~3 min) · Tests: `dotnet test KackZug.slnx`
- Desktop dev head: `dotnet run --project src/KackZug.Desktop`
- Emulator: `%LOCALAPPDATA%\Android\Sdk\emulator\emulator.exe -avd qwocoo_test -no-window -gpu swiftshader_indirect -no-audio -no-snapshot-save`
  (AVD belongs to the developer's Qwocoo project, API 35; reuse, don't modify)
- Install + start: `adb install -r src/KackZug.Android/bin/Debug/net10.0-android/io.github.bastianbaumann.kackzug-Signed.apk`,
  `adb shell monkey -p io.github.bastianbaumann.kackzug -c android.intent.category.LAUNCHER 1`, `adb logcat -d`
  (adb: `%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe`, not on PATH)
- Git: `main` (initial commit), daily work on `dev`.
- Screenshot tour (Debug desktop): set `KACKZUG_SCREENSHOTS=<dir>`, `KACKZUG_DATA_DIR=<isolated dir>`, `KACKZUG_DEMO=1`, then `dotnet run --project src/KackZug.Desktop`; writes PNGs + checks.txt, closes itself. Extend `src/KackZug.Desktop/TourSteps.cs` per view.
