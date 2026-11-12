# Task Board: KackZug

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

**Legend:** [ ] open · [/] in progress · [~] review · [x] done · [!] blocked · [?] needs clarity · [-] descoped
**Closed milestones:** one line each below; details in ./archive/Taskboard_archive.md

## Milestones
| Milestone | Epics | Goal | Status |
|---|---|---|---|
| M9 v0.5.1 | E-13 | Tutorial Einzelfahrten, geteilte Zuege verfolgen, Grund bei Keine Daten | released v0.5.1 (2026-10-22) |
| M8 Einzelfahrten | E-12 | Einzelfahrten mit Tab Fahrten, Ton/Vollbild abschaltbar, Jetzt pruefen fuer Fahrten, klarere Auswahl beim Speichern | released v0.5.0 (2026-10-22) |
| M7 Weiter v0.4.2 | E-11 | Keine Daten statt Teilausfall, Badge je Fahrt, ruhiger Tag, keine Ergebnisliste, Speicher aufraeumen | released v0.4.2 (2026-10-22) |
| M6 Feedback v0.4.0 | E-10 | Sprache live, Erlauben-Knopf, Vorabversionen nur von dev, Alarm-Bildschirm, Startbildschirm, leise Erinnerungen, Doppelpunkt, Gespeichert | released v0.4.1 (2026-10-21) |
| M5 Feedback+Updater | E-09 | Teilausfall-Fix, gruene Meldungen, Navigator fehlt, Englisch, Updater | released v0.4.0 (2026-10-21) |
| M3 Planung+ | E-07 | Naechste Woche, ganze Woche setzen, Fahrt-Detail, Navigator, Zeit-/Zahlenfelder, Eingabe von Hand, Bitte nicht stoeren, eigener Ton | released v0.3.0 (2026-10-21) |
| M2 Feedback 1 | E-06 | Design System uebernehmen, Dauer-Alarm bis "Gesehen" | open |
| M1 MVP | E-01..E-05 | Schichten per Teilen anlegen, Woche planen, Pruefung + lauter Alarm auf dem Handy, signiertes Release auf GitHub, Tutorial | open |

**Exit checklist (every milestone):** all tasks [x]/[-] · CI green · README/runbook/design agree · no open
blockers/questions · proposed ADRs resolved · compaction done · STATE + SessionLog updated.

## Ownership map
| Role | Owns paths/areas |
|---|---|
| Core | src/**, tests/**, assets/**, .github/**, README.md, LICENSE, .gitignore, .editorconfig |
| Dev/Bastian | release keystore + GitHub secrets, real-device tests, real share links |

Critical path: T-001 -> T-004 -> T-005/T-006 -> T-008 -> T-012/T-015 -> T-016 -> T-017 -> T-021.

- M1 / E-01 Geruest done 2026-10-20: T-001, T-003 [x], T-002 [-] (logo replaced by design system) -> archive/Taskboard_archive.md

- M1 / E-02 Kernlogik done 2026-10-20: T-004..T-009 [x], 71 tests at close -> archive/Taskboard_archive.md

- M1 / E-03 Oberflaeche done 2026-10-20: T-010..T-014 [x] -> archive/Taskboard_archive.md

- M1 / E-04 Android-Hintergrund done 2026-10-20: T-015, T-016 [x] -> archive/Taskboard_archive.md

## M1 / E-05: Release, Updates, Tutorial
- Done, details in archive/Taskboard_archive.md (compacted 2026-10-21): T-017 Release workflow, T-018 Create release keystore, back it up offl, T-019 Update banner in app (opens release page, T-020 Tutorial on first start and after each u, T-021 README (Deutsch
- [x] **T-022** Real-device test: share from DB Navigator, alarm while locked, reboot, update install · owner: @Dev/Bastian · depends-on: T-017
  - **DoD:** developer confirms each point.
  - [2026-11-10] (@Dev/Bastian) DONE: erledigt

## M2 / E-06: Feedback-Runde 1 (2026-10-20)
- [x] **T-023** Adopt Design System "ZugKacke": app icon (real orange background instead of DB red #EC0016, Q-006), wordmark as "KackZug" (Q-007), notification small icon, tokens -> Tokens.axaml (light/dark), Inter font, its icon set, component styles, text tone · owner: @Core · depends-on: none · origin: dev request 2026-10-20, D-009, Q-006, Q-007
  - **DoD:** launcher icon, in-app logo and notification icon come from the design system assets; colours/type from tokens.json; screenshot tour + emulator check; developer approves look.
  - **Log:**
    - [2026-10-20] (@Core) DONE: assets/logo (design system app icon with orange background, mark, notification) + assets/icons (16 design system icons + own plus/trash in same style); tools/design/build_assets.py (replaces tools/logo) generates Android adaptive icon, notification icon, Avalonia LogoImage/LogoMarkImage, Icons.axaml; Themes/Tokens.axaml (light/dark, D-010), OutlineIcon control, AppStyles per bundle.css (primary/secondary/ghost/destructive buttons, day rows, badges, trip toggles, choice cards, segments, setting rows), Inter font, all views restyled (functions unchanged), wordmark "KackZug" with "Kack" in brand, problem titles upper case, CheckState.LastBadgeText, toast tone "Gespeichert. Ich pass auf.".
    - [2026-10-20] (@Core) VERIFY: 85/85 tests; tour 47/47 + screenshots light/dark of all pages; emulator: Inter, orange launcher icon, week page · not verified: developer approval of the look, notification small icon visually
    - [2026-11-10] (@Dev/Bastian) DONE: abgenommen
- Done, details in archive/Taskboard_archive.md (compacted 2026-10-21): T-024 Persistent alarm until "Gesehen", T-025 Bug, T-026 Bug, T-027 Checks must be honest, T-028 Bug
- [-] **T-029** Alarms lost after swiping the app away on the developer's phone · owner: @Core · origin: dev device test 2026-10-20 · depends-on: Q-008
  - **Log:**
    - [2026-10-20] (@Core) PROGRESS: reproduced developer's flow on stock Android 15 emulator with the Release APK: swipe -> "Killing ... remove task", test alarm still fired 2 s later via cold start ("Start proc ... for broadcast"), notification shown. So the code survives a swipe; the phone's vendor layer (swipe = force stop) clears alarms unless the app is whitelisted (battery unrestricted / autostart). Qwocoo likely whitelisted already.
- [x] **T-030** Feedback 2026-10-21: slow tab switch, stretched number fields, theme segments multi-select · owner: @Core · origin: dev device test 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: ViewLocator caches page views; ContentControl without transition; week rebuilds only on data/day change and not after its own toggle saves; shifts no rebuild on show; NumericUpDown/TimePicker fixed 48 px centred; theme, Hin/Rueck and import targets are RadioButtons (ToggleButton theme, GroupName); release.yml marks tags with "-" as pre-release (version without suffix).
    - [2026-10-21] (@Core) VERIFY: 87/87 tests, tour 53/53 incl. real radio clicks, field height, view reuse; pushed dev 1bff57b, main untouched (developer: app is being tested) · not verified: speed on the developer's phone

## M3 / E-07: Planung und Details (2026-10-21, developer choice B1, B2, B4; dev branch only)
- [x] **T-031** Plan next week: WeekSwitcher "Diese Woche / Naechste Woche", AppData.NextWeek (created on first open: copy or empty), rollover takes NextWeek, checks use it · owner: @Core · origin: Backlog B1 · claimed: @Core @ 2026-10-21
  - **DoD:** tests for rollover with planned next week, trip calendar for next-week dates, shift removal frees next week; tour: switch, edit next week, persists.
  - **Log:**
    - [2026-10-21] (@Core) DONE: AppData.NextWeek + EnsureNextWeek (copy/empty per setting), WeekPlan.CreateFrom/Contains, rollover adopts planned next week and drops outdated ones, free/remove shift in both weeks, TripCalendar prefers NextWeek; WeekViewModel IsThisWeek/IsNextWeek radio segment, title.
    - [2026-10-21] (@Core) VERIFY: 92/92 tests (+5), tour 58/58 incl. switch, edit, persisted · commit on dev
- [x] **T-032** Set several days at once: selection mode in the week (button, long press where possible), choose shift, apply · owner: @Core · depends-on: T-031 · origin: Backlog B2 · claimed: @Core @ 2026-10-21
  - **DoD:** tour: select 3 days, apply shift, saved; past days not selectable.
  - **Log:**
    - [2026-10-21] (@Core) DONE: WeekViewModel StartSelecting/CancelSelecting/ApplySelection with bulk shift picker bar, DayRow IsMarked/CanMark/ApplyShift (enables both trips), WeekView Holding handler for long press, refresh suppressed while selecting.
    - [2026-10-21] (@Core) VERIFY: tour 60/60 (3 days set + saved, past not markable); long press verified on emulator during T-033
- [x] **T-033** Trip detail: tap a day -> trips with big time + badge, live state per leg ("jetzt 06:54", platform, cancelled struck through), refresh live; Android back closes it · owner: @Core · depends-on: T-031 · origin: Backlog B4 · claimed: @Core @ 2026-10-21
  - **DoD:** tour with demo source: detail shows +8 and transfer warning per leg; back closes.
  - **Log:**
    - [2026-10-21] (@Core) DONE: TripDetailViewModel/TripCardViewModel/LegDetailLine (TimeDisplay style: 40 px time, badge, "jetzt HH:mm", strikethrough on cancel, per-leg badge + "ab/an HH:mm", platform, "noch keine Echtzeit", transfer notes), auto live check for today/tomorrow, "Live pruefen"; MainViewModel.HandleBack via TopLevel.BackRequested (dialog, import, detail, selection, editor, other tab -> week).
    - [2026-10-21] (@Core) VERIFY: tour 64/64 (+8, jetzt 05:50, transfer warning, back closes); emulator: long press starts selection, Android back cancels it, Details opens, back closes and app stays · commit on dev

- [x] **T-034** Trip detail: button "Im DB Navigator oeffnen" per trip (prefilled bahn.de search for that day, DB Navigator if it takes the link, else browser) · owner: @Core · depends-on: T-033 · origin: dev test 2026-10-21 · claimed: @Core @ 2026-10-21
  - **DoD:** link builder unit-tested (stations, date/time, encoding); button in detail card; tour checks the opened URL.
  - **Log:**
    - [2026-10-21] (@Core) DONE: Core/Bahn/NavigatorLink (bahn.de/buchung/fahrplan/suche#so/zo/soid/zoid/hd=day+first departure, hza=D), IPlatform.OpenInNavigator (Android: ACTION_VIEW with package de.hafas.android.db if it resolves, else plain ACTION_VIEW), TripCardViewModel.NavigatorUrl + OpenInNavigatorCommand, secondary button per trip card, README line.
    - [2026-10-21] (@Dev/Bastian) VERIFY: search link opened the browser, not the app -> T-038 launches the app instead
    - [2026-10-21] (@Core) VERIFY: 114/114 tests (+3 link), tour 71/71 (link for the day, button visible), Android build ok · not verified: whether DB Navigator takes the search link and lands on the right connection (bahn.de blocks the agent browser as bot, Navigator not on emulator) -> developer phone; old vbid is not stored and would show the import day anyway
- [x] **T-035** Settings: evening check time as plain text field "HH:mm" instead of the TimePicker dropdowns · owner: @Core · origin: dev test 2026-10-21 ("sieht immernoch komisch aus") · claimed: @Core @ 2026-10-21
  - **DoD:** accepts 19:30 / 19.30 / 1930 / 19; invalid input shows a hint and keeps the old time; parser unit-tested; tour types a time and it is saved.
  - **Log:**
    - [2026-10-21] (@Core) DONE: Core/Planning/ClockText (parse/format), SettingsViewModel.EveningCheckTimeText/Invalid saves on every valid input, TidyEveningCheckTime on LostFocus, TextBox.time style (96x48, centred, tabular digits, number keyboard), TimePicker removed.
    - [2026-10-21] (@Core) VERIFY: 114/114 tests (+19 cases), tour 71/71 (1930 saved as 19:30, 25:00 shows hint and keeps 19:30) + screenshot · not verified: number keyboard on the developer's phone (if ":" is missing, "1930" works)

- [x] **T-036** Optional: problem alarms may break through "Bitte nicht stoeren" (setting, DND channel with bypass, notification policy access in permissions) · owner: @Core · origin: Backlog B6, dev request 2026-10-21 · claimed: @Core @ 2026-10-21
  - **DoD:** setting off by default; on + access granted -> alarms posted on a bypass channel; access missing -> permission row opens the policy access screen; desktop tour toggles and saves.
  - **Log:**
    - [2026-10-21] (@Core) DONE: Settings.BreakThroughDoNotDisturb (default off), ACCESS_NOTIFICATION_POLICY in manifest, AlertNotifier.ProblemChannelFor -> channel stoerung_dnd_v1 with SetBypassDnd only created once access is granted (Android ignores bypass otherwise), AlarmFlow posts on it, permission row "Bitte nicht stoeren" (only when setting on) opens ActionNotificationPolicyAccessSettings, toggle opens it directly when missing, README.
    - [2026-10-21] (@Core) VERIFY: Android build ok; tour: toggle saved and visible · not verified: real phone in DND (emulator not run this time); note: alarms already use category alarm + alarm usage, so DND with "Alarme erlaubt" may have let them through before
- [x] **T-037** Manual connection entry as fallback: legs with station search (bahn.de orte), times, line, optional train number; then the same assign step as the share import · owner: @Core · origin: Backlog B3, dev request 2026-10-21 · claimed: @Core @ 2026-10-21
  - **DoD:** station search parsed from fixture (tests); validation (stations chosen, times, arrival after departure, legs connect); board matching tolerates "RE 1" vs "RE1"; tour enters a 2-leg connection and saves it to a shift.
  - **Log:**
    - [2026-10-21] (@Core) DONE: Core: BahnWebClient.FindLocationsAsync (reiseloesung/orte, checked once live: 200, extId/name/type), StationSearchParser (type ST, dedupe), IStationSearch/BahnWebStationSearch, ManualLegInput/ManualConnectionBuilder/ManualBuildResult (ClockText times, day offsets over midnight, walk leg between different stops, German messages per leg), BoardMatcher compares names/lines without spaces and case. App: ImportViewModel manual step (StartManual, EditManual, hint), ManualEntryViewModel/ManualLegViewModel/StationPickerViewModel/StationResult, ManualEntryView, entry points: shifts list button + "Von Hand eingeben" on import error. Design deviation: search on button press, not while typing (bahn.de rate limit).
    - [2026-10-21] (@Core) VERIFY: 126/126 tests (+12), tour 90/90 (empty error, search, chain stations, walk inserted, edit keeps values, saved as Nachtdienst, error -> manual) + screenshots · not verified: real bahn.de search in the app, live check of a hand-entered trip on the phone
- [x] **T-038** Feedback v0.2.2-dev.2: Navigator button opened the browser; delay/transfer still up-down fields · owner: @Core · origin: dev test 2026-10-21 ("App nicht browser", "die beiden sollen auch einfache eingabefelder sein")
  - **Log:**
    - [2026-10-21] (@Core) DONE: AndroidPlatform.OpenInNavigator launches de.hafas.android.db like the notification action (bahn.de link only without Navigator; the app does not take the search link, so it opens on its start page, not on the connection); delay (1-60) and transfer (0-30) as TextBox.number with hint on invalid input and tidy on lost focus, NumericUpDown styles removed.
    - [2026-10-21] (@Core) VERIFY: tour 90/90 (no NumericUpDown left, 8 saved, 0 shows hint and keeps 8, 6 saved), Android build ok · not verified: on the phone
    - [2026-10-21] (@Dev/Bastian) VERIFY: number fields OK, test alarm rings in DND (T-036), manual entry works and reported "planmaessig" (T-037); Navigator opens but only its start page -> Q-009

- [x] **T-039** "Laut Fahrplan" badge green (Ok colour) instead of grey; grey only for not checked / unknown · owner: @Core · origin: dev test 2026-10-21 (overrides the grey choice from T-027) · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: StatusBadge.VariantOf PlannedOnly -> Ok (week badge and trip detail); text stays "Laut Fahrplan"; Unknown and null stay grey.
    - [2026-10-21] (@Core) VERIFY: tour check "Laut Fahrplan gruen, nicht geprueft grau" · not verified: look on the phone
- [x] **T-040** Own, more annoying problem alarm tone (generated, reproducible script in tools/), new channel ids because Android channels keep their sound · owner: @Core · origin: dev test 2026-10-21 ("Weckerton des Telefons ... nervigeren, auffaelligeren Ton") · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: tools/sound/build_alarm.py -> Resources/raw/kackzug_alarm.wav (2 s: 1000/1500 Hz band-limited square warble x6, 3 beeps at 2200 Hz, 0.26 s pause; 16 bit mono 22.05 kHz, 88 KB); channels stoerung_v2 / stoerung_dnd_v2 with android.resource sound, old v1 channels deleted on start; README.
    - [2026-10-21] (@Core) VERIFY: Android build ok; WAV sent to developer to listen · not verified: sound and loudness on the phone, DND channel recreated after update (access already granted)
    - [2026-10-21] (@Dev/Bastian) VERIFY: new alarm tone works on the phone ("klappt super")
- [x] **T-041** Settings button "Nach Updates suchen": manual GitHub check now, result shown (new version banner / up to date / could not check) · owner: @Core · depends-on: T-019 · origin: dev request 2026-10-21 · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: UpdateChecker.CheckAsync -> UpdateCheckResult (NewVersion / UpToDate / Failed), FindNewerAsync uses it; AppServices.CreateUpdateChecker (UpdateClientFactory for the tour); SettingsViewModel.CheckUpdatesNowCommand with status text, banner via MainViewModel.ShowUpdate; README.
    - [2026-10-21] (@Core) VERIFY: 127/127 tests (+1), tour 95/95 (up to date, 404 -> could not check, v9.9.9 -> banner) + screenshot · finding: api.github.com/repos/BastianBaumann/KackZug/releases/latest answers 404 without login (repo private), so the real check can only work once the repo is public; pre-releases are never offered (releases/latest), see Q-010

    - [2026-10-21] (@Dev/Bastian) VERIFY: button works as expected (could not check while repo is private)
- [x] **T-042** Navigator button opens the exact connection for that day: search on bahn.de (angebote/fahrplan), pick the saved connection, create a share link (angebote/verbindung/teilen -> vbid), open bahn.de/buchung/start?vbid=... in DB Navigator; fallback: app start page · owner: @Core · depends-on: T-038 · origin: dev test 2026-10-21, Q-009 · claimed: @Core @ 2026-10-21
  - **DoD:** picker + share request unit-tested with invented fixtures; live probe of both endpoints recorded; button shows progress and falls back with a short message; tour covers success and fallback.
  - **Log:**
    - [2026-10-21] (@Core) PROGRESS: live probe (3 requests): POST angebote/fahrplan -> 201, verbindungen[].ctxRecon + verbindungsAbschnitte (abfahrt.sollzeit, verkehrsmittel.nummer); POST angebote/verbindung/teilen {startOrt, zielOrt, hinfahrtDatum, hinfahrtRecon} -> 201 {vbid}; GET verbindung/{vbid} -> recon for the requested day; hinfahrtDatum without offset is read as UTC -> send with offset. Samples: specs/samples/fahrplan-1.json, teilen-1.json, verbindung-3.json
    - [2026-10-21] (@Core) DONE: Core/Sharing (IShareLinkCreator, ShareLinkCreator: search at first ride on the day -> ConnectionPicker matches rides by stop + time (+ number if both known) -> CreateShare with Berlin offset -> bahn.de/buchung/start?vbid=), BahnWebClient.SearchConnectionsAsync/CreateShareAsync, ReconResponseMapper.MapConnection, StationIds; NavigatorLink (search link) removed; IPlatform.OpenInNavigator(string?) (Android: link with Navigator package, else browser; null -> launch app); TripCardViewModel async command with "Verbindung wird vorbereitet", link cached per card, toast + app start page when not found; README.
    - [2026-10-21] (@Core) VERIFY: 130/130 tests (+6 sharing, -3 old link), tour 97/97 (fallback, link for the day, created once), Android build ok; live run of ShareLinkCreator (temporary test, deleted): Erfurt Hbf 06:52 RE 3903 on 2026-10-22 -> vbid link · not verified: Navigator opening the generated link on the phone
    - [2026-10-21] (@Core) PROGRESS: dev pushed, pre-release tag v0.2.2-dev.5 on dev @ b75de4e pushed (developer OK: "Ja, push dev und tagge v0.2.2-dev.5."); main untouched
    - [2026-10-21] (@Dev/Bastian) VERIFY: hand-entered and imported connections open correctly in DB Navigator ("perfekt", "ohne probleme")
    - [2026-10-21] (@Core) PROGRESS: main fast-forwarded to b75de4e and tag v0.3.0 pushed (developer OK: "Das kann so also gern auf den Main branch, und tagge gleich einen neuen apk Build, Die 0.3.0")

## M4 / E-08: Oeffentliches Repo (2026-10-21)
- [x] **T-043** Crash when sharing into KackZug while it runs in the background: second MainActivity in the Navigator task got the app-wide cached page views ("already has a visual parent"); fix: ViewLocator per MainView, MainActivity SingleTask, tour step with a second MainView · owner: @Core · origin: dev bug report 2026-10-21 · claimed: @Core @ 2026-10-21
- [x] **T-044** Make the repo ready for outside contributions: CONTRIBUTING, SECURITY, CODE_OF_CONDUCT, issue/PR templates, CODEOWNERS, dependabot, rulesets (main/dev/v* tags), CI read-only, release only via environment "release" and stable tags only from main · owner: @Core · origin: dev request 2026-10-21, Q-010 · claimed: @Core @ 2026-10-21
  - developer applied the GitHub settings 2026-10-21 (public, rulesets, environment release); secrets stay repository secrets (values not available)
  - developer confirmed the share fix on v0.3.1-dev.1 2026-10-21

## M5 / E-09: Feedback v0.3.1, Englisch, Updater (2026-10-21) · released v0.4.0
- [2026-10-21] (@Dev/Bastian) VERIFY: "der jetzige Stand ist sauber" after dev.1 -> dev.2 -> dev.3 on the phone; dev = main = 0c67a87, tag v0.4.0 pushed (developer OK)
- [x] **T-045** Bug: a partial outage today was shown for today and tomorrow · owner: @Core · origin: dev test 2026-10-21 (RE1 17:38 Jena West) · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) PROGRESS: live probe ~18:30 (6 calls): RE1 3664 board + trip today and tomorrow are clean again (full stop list, no notes), so the cause was transient. Only path that reports a partial cancellation for a clean stop list: free-text "fällt aus"/"Halt entfällt" in stop notes; general disruption messages (HIM, priorisierteMeldungen) hang on every trip of the line while they last -> also tomorrow.
    - [2026-10-21] (@Core) DONE: CancellationNotes reads text only from trip notes (risNotizen, meldungen), not from priorisierteMeldungen or type HIM; structured signals (cancelled, key text.realtime.*, HALT_AUSFALL) still count everywhere.
    - [2026-10-21] (@Core) VERIFY: 132/132 tests (+2: HIM text no cancellation, RIS stop note still cancellation) · not verified: the original message text (not captured); watch the next real disruption
- [x] **T-046** Bug: notification always red, also when it says everything is on time; info notifications green (ok) / grey (unknown), problem stays red; a problem alarm that turned ok stops repeating · owner: @Core · origin: dev test 2026-10-21 · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: AlertNotifier info notifications SetColor ok #1B7A35 / unknown #56606B (before: no colour, phone used its own); an info for the same trip acknowledges a stored problem alarm first (before, the repeat re-posted the old red alarm with its old text after the trip was fine again); AlarmFlow uses AlertNotifier.ProblemColor.
    - [2026-10-21] (@Core) VERIFY: Android build ok · not verified: colour on the phone (test alarm is a problem alarm and stays red by design)
- [x] **T-047** Navigator button without DB Navigator installed: say so and open bahn.de in the browser, never crash when no app takes the link · owner: @Core · origin: dev question 2026-10-21 · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: before: link opened in the browser silently (no crash; without a link bahn.de start page); a phone without any browser would have crashed (ActivityNotFoundException). IPlatform.OpenInNavigator returns false when the browser was used; toast "Der DB Navigator ist nicht installiert ..."; StartActivity guarded in AndroidPlatform and AlarmFlow.OpenNavigator.
    - [2026-10-21] (@Core) VERIFY: tour 101/101 (+1: without Navigator browser opens with hint), Android build ok · commits 9357f0a, ddc8988, 75a1e7e on dev (local)
- [x] **T-048** App in English and German (setting, default system language) and README in English and German · owner: @Core · origin: dev request 2026-10-21, D-011 · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) PROGRESS: groundwork be42564 on dev: AppLanguage + Settings.Language, Core/Localization Strings.resx (en) + Strings.de.resx, Language.Apply/Resolve, settings segment System/Deutsch/English, App.ReloadUi (desktop: new MainView; Android: activity Recreate), BackgroundCheck applies the stored language. Mode B for the bulk: subagent 1 Core+Android texts (main tree), subagent 2 App+tour (worktree, own commit), subagent 3 README.md (en) + README.de.md.
    - [2026-10-21] (@Core) PROGRESS: 35d3638 Core+Android texts (82 keys, channel names follow the language, receivers apply the stored language; 143 tests); 223cb46 README.md English + README.de.md, CONTRIBUTING/PR template mention both languages. App+tour still running (worktree).
    - [2026-10-21] (@Core) DONE: d33f3bc App texts (UiStrings 186 keys, UiLanguage, dates per language, tour starts German + English switch step), ae1b923 umlaut fix.
    - [2026-10-21] (@Core) VERIFY: 143/143 tests, tour 112/112 (incl. switch to English and back), screenshots week-en/settings-en checked, Android build ok · not verified: language switch (activity recreate) and English notifications on the phone
    - [2026-10-21] (@Core) NOTE: developer: hold the language changes back (not merged) to test the auto update with them. Parked on branch feature/language @ ae1b923 (5 commits); dev reset to 75a1e7e (local only, never pushed).
    - [2026-10-21] (@Core) PROGRESS: developer OK to rebase: feature/language now on feature/updater, tip 37d9d7d (23df72a, 875f09d, ff16b5f, 9605815, 3abd42e + 37d9d7d updater texts EN/DE, banner survives the language switch).
    - [2026-10-21] (@Core) VERIFY: 147/147 tests, tour 136/136 (English banner "Version 9.9.9 is here." / "Open download page", "Offer pre-releases"), week-en screenshot checked, Android build ok
    - [2026-10-21] (@Dev/Bastian) VERIFY: after the self-update dev.1 -> dev.2 "Teilausfall" etc. still German; Play Protect showed "harmful app blocked" during the update
    - [2026-10-21] (@Core) DONE: cause: badge texts are stored with the check (CheckState.LastBadgeText) in the language of that moment. CheckState.LastBadgeLanguage (empty = old German data), StatusBadge.TextFor falls back to the short text in the current language; commit 0c67a87 on feature/language (local)
    - [2026-10-21] (@Core) VERIFY: 147/147 tests, tour 137/137 (+1: German stored badges show in English), Android build ok · not verified: on the phone
    - [2026-10-21] (@Dev/Bastian) VERIFY: Play Protect "harmful app blocked" already on the manual install of KackZug-0.4.0-dev.1.apk and again on the in-app update; "More details -> Install anyway" works in both cases; in-app update dev.1 -> dev.2 works on the phone
- [x] **T-049** Real in-app updater (download + install itself), built as a reusable module for other apps, kept in the AgentStarter repo; own branch · owner: @Core · origin: dev request 2026-10-21, D-012 · claimed: @Core @ 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) PROGRESS: module design: AgentStarter/modules/app-updater (branch feature/app-updater there): AppUpdater (net8.0: AppVersion, IUpdateSource, GitHubReleaseSource with pre-release option + sha256 from asset digest or .sha256 file, UpdateDownloader with progress + checksum, UpdateService state machine, IUpdateInstaller) + AppUpdater.Android (PackageInstaller session, silent on API 31+ once KackZug is installer of record, confirmation otherwise) + tests; subagent builds it. KackZug integration afterwards on branch feature/updater.
    - [2026-10-21] (@Core) PROGRESS: module done by subagent: AgentStarter branch feature/app-updater @ 80d7d4e (local, not pushed), build 0 errors incl. Android, 76 tests; REQUEST_INSTALL_PACKAGES + receiver come from the library; downloader refuses releases without checksum (GitHub asset digest or .sha256). KackZug release.yml already raises versionCode per release (minutes since 2026-01-01). Not verified: on a device.
    - [2026-10-21] (@Core) PROGRESS: KackZug branch feature/updater from dev @ ae1b923; subagent vendors the module (src/AppUpdater, src/AppUpdater.Android), replaces Core/Updates, pre-release toggle, download/install banner, release.yml .sha256, tour + README.
    - [2026-10-21] (@Core) NOTE: rebased plan: feature/updater moves onto 75a1e7e (no language), German literals; test plan: release dev+updater as pre-release, install, then release feature/language as a newer pre-release and let the app update itself.
    - [2026-10-21] (@Core) DONE: feature/updater @ dc2a041 on 75a1e7e (9ab72b8 vendor module, 4c5364c controller/banner/permission flow/pre-release toggle, 214b2ea .sha256 asset, 86bde8e InformationalVersion = full tag version so dev builds compare right, dc2a041 README); old Core/Updates removed; daily check due after 20 h.
    - [2026-10-21] (@Core) VERIFY: 136/136 tests, tour 121/121 (up to date, 404, pre-release only with toggle, desktop opens page, checksum mismatch, progress 100 %, permission dialog, resume installs, abort + retry without new download), screenshot update-downloading checked, Android build ok, manifest has REQUEST_INSTALL_PACKAGES + InstallStatusReceiver · not verified: PackageInstaller on a phone, silent install on Android 12+, real release digest/.sha256
    - [2026-10-21] (@Core) PROGRESS: dev fast-forwarded to dc2a041 and pushed, tag v0.4.0-dev.1 pushed (developer OK); phone install pending
    - [2026-10-21] (@Dev/Bastian) VERIFY: self-update dev.1 -> dev.2 in the app works on the phone; Play Protect warns "harmful app blocked" on the APK itself (also on manual install), "Install anyway" works -> KnownIssues, appeal/developer verification is the developer's call

## M6 / E-10: Feedback v0.4.0 (2026-10-21) · released v0.4.1
- [2026-10-21] (@Dev/Bastian) VERIFY: developer released it: "bring die 0.4.1 auf main, und release sie als richtige version"; main = dev = 229f6fb, tag v0.4.1
- [x] **T-050** Language switch did nothing on Android until restart · owner: @Core · origin: dev test v0.4.0 (system English, switch to Deutsch)
  - **Log:**
    - [2026-10-21] (@Core) DONE: cause reproduced on the emulator: Avalonia's Android lifetime is also an ISingleViewApplicationLifetime, so ReloadUi set singleView.MainView, which Android never shows. Activity content is now a ContentControl host; ReloadUi swaps its content first (no activity recreate). Commit a7b8f19.
    - [2026-10-21] (@Core) VERIFY: emulator (API 35, debug): German -> English switches at once, settings page in English, Android back still goes to the week; 147 tests, tour 137/137 · not verified: on the developer's phone
- [x] **T-051** Permission "Erlauben" looked like plain text · owner: @Core · origin: dev test v0.4.0
  - **Log:**
    - [2026-10-21] (@Core) DONE: Button.primary.compact (filled accent, 40 px) instead of ghost; commit c75f96e. VERIFY: emulator screenshot (filled orange "Allow" next to "Missing")
- [x] **T-052** Pre-releases only from dev, development on branches · owner: @Core · origin: dev question 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: release.yml refuses a tag with "-" whose commit is not on origin/dev (stable still main only); CONTRIBUTING says so; commit 258d581. Rulesets already: only admins create v* tags, dev needs PR + code owner review + CI. Work for this round on fix/settings-feedback, fast-forwarded into dev. · not verified: a real tag run with the new check

- [x] **T-053** Alarm screen: "back" on the lock screen closed the full-screen alarm and the tone stopped (only the small notification stayed) · owner: @Core · origin: dev test v0.4.0
  - **Log:**
    - [2026-10-21] (@Core) DONE: cause: target SDK 36 has predictive back, the old OnKeyDown(Back) block no longer gets the key. AlarmActivity registers an OnBackInvokedCallback (PriorityOverlay, does nothing) + empty OnBackPressed; OnStop without "Gesehen" while the screen is on reposts the alarm (insistent tone again, full screen again when locked); screen off -> no repost (the scheduled repeat brings it back). Commit 75bae29.
    - [2026-10-21] (@Core) VERIFY: emulator API 35: test alarm with screen off -> AlarmActivity on top; back -> still on top; home -> notification reposted, dumpsys mSoundNotificationKey = our alarm; 147 tests · not verified: real lock screen with PIN, the tone itself (emulator without audio), power button behaviour on the phone

- [x] **T-054** Start screen with the logo, light/dark per app setting, no black gap, smoother start · owner: @Core · origin: dev request 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: Android 12+ system splash (launcher icon on splash_background, values-night dark); API 26-30 windowBackground layer-list; IPlatform.ApplyThemeMode -> UiModeManager.SetApplicationNightMode so the system splash follows the app setting; SplashOverlay on the DecorView (same look) fades out once the Avalonia SurfaceView has real pixels (PixelCopy; Avalonia's frame callback fired ~6 s before the first visible frame on the debug emulator); commit 31cde3a.
    - [2026-10-21] (@Core) VERIFY: emulator API 35 cold starts: light: splash 0.8-12 s then app, no black frame (before: ~4 s black); system dark: dark splash; app "Light" + system dark: light splash; tour 137/137, 147 tests · not verified: release build timing on the phone, API 26-30 look
- [x] **T-055** On-time reminders without sound · owner: @Core · origin: dev request 2026-10-21
  - **Log:**
    - [2026-10-21] (@Core) DONE: info channel info_v2 without sound and vibration (channels keep their sound, so new id; info_v1 deleted on start); README both languages; commit 07abfd8. VERIFY: emulator dumpsys: info_v2 mSound=null, vibration off, info_v1 deleted

- [x] **T-056** Time fields: no colon on the phone's number keyboard -> colon comes by itself (2230 -> 22:30) · owner: @Core · origin: dev test v0.4.1-dev.2
  - **Log:**
    - [2026-10-21] (@Core) DONE: ClockText.FormatTyping (colon with the first minute digit, hour one digit if it starts with 3-9, 25 stays visible as invalid), Controls/TimeInput attached to TextBox.time via style (settings + manual entry); hooks TextProperty changes, caret moved to the end again via Dispatcher.Post because the TextBox sets it after the input; commit f0f1239.
    - [2026-10-21] (@Core) VERIFY: 165 tests (+18), tour 139/139 (+1); emulator number keyboard: 2230 -> 22:30 (first try gave 22:03, fixed with the deferred caret), delete twice -> 22, type 15 -> 22:15
- [x] **T-057** Feedback that a setting was saved · owner: @Core · origin: dev test v0.4.1-dev.2
  - **Log:**
    - [2026-10-21] (@Core) DONE: SettingsViewModel.ConfirmSaved -> green "Gespeichert"/"Saved" chip at the bottom of the settings page, fades/slides in, 1.6 s, restarts on each change; all save paths incl. theme, colour, DND, language (after rebuild); commit 229f6fb.
    - [2026-10-21] (@Core) VERIFY: tour check "Aenderung zeigt kurz Gespeichert"; emulator screenshot after toggling the evening check

## M7 / E-11: Weiter fuer v0.4.2 (2026-10-21) · released v0.4.2
- [2026-10-22] (@Dev/Bastian) VERIFY: "sonst ist alles super, bring es also auf main und bring den release"; main = dev = 71da6d7, tag v0.4.2; B7 (smaller APK) declined for now
- [x] **T-058** "Teilausfall" shown when bahn.de had no usable data · owner: @Core · origin: dev test v0.4.1
  - **Log:**
    - [2026-10-21] (@Core) DONE: cause: TripReader set ArrivalCancelled when the destination stop was missing from the trip's stop list (incomplete data or another stop id), which became PartlyCancelled (red alarm). Now LegObservation.ArrivalMissing -> LegStatus.Unknown (grey, info, no alarm); real stop cancellations still come from cancellation notes on the stop. Unknown texts "Nicht geprüft"/"Not checked" -> "Keine Daten"/"No data", problem text "keine Daten von bahn.de"; README both languages; commit 0fdb178.
    - [2026-10-21] (@Core) VERIFY: 166 tests (+1, 3 adapted), tour 139/139, Android build ok · not verified: the developer's original case on the phone (which leg/stop was missing is unknown)

- [x] **T-059** Week view showed only the worse status of the day (outbound ok + return no data -> only "Keine Daten") · owner: @Core · origin: dev test v0.4.2-dev.1
  - **Log:**
    - [2026-10-21] (@Core) DONE: day badge next to the shift removed, each trip toggle (Hin/Rueck) gets its own badge below it; DayRowViewModel.DayBadge/WorstBadge removed; commit on dev.
    - [2026-10-21] (@Core) VERIFY: 166 tests, tour 140/140 (+1), screenshots week-checked and week-en (badges under each trip, fits at 400 px) · not verified: on the phone

- [x] **T-060** Week day: toggling Hin/Rueck moved the rest of the day; layout felt messy -> tidy, steady day card · owner: @Core · origin: dev test v0.4.2-dev.2
  - **Log:**
    - [2026-10-21] (@Core) DONE: day card = header (shift chip left, "Details" right) + one fixed 52 px line per trip: toggle left, right side badge + "geprueft HH:mm" (or "Aus" when switched off); captions below removed (VM keeps OutboundCaption/ReturnCaption), new OutboundChecked/ReturnChecked/OutboundOff/ReturnOff, strings Day_CheckedAt, Day_TripOff; BadgeTemplate uses a Grid so long badge texts end in "..." instead of overflowing; deviation from design system DayRow (one worst badge at the right) by developer request (T-059/T-060).
    - [2026-10-21] (@Core) VERIFY: tour 142/142 (+2: card height unchanged after switching a trip off, checked time shown), screenshots week-today/week-dark/week-en at 400 px; 166 tests; Android build ok · not verified: phone
    - [2026-10-21] (@Core) NOTE: design system artifact DayRow README still describes one badge per day; update offered to the developer

- [x] **T-061** Remove the result list under "Jetzt pruefen"; the badges in the days are enough · owner: @Core · origin: dev test v0.4.2-dev.3 (2026-10-22)
  - **Log:**
    - [2026-10-22] (@Core) DONE: WeekViewModel.CheckResult and its box removed; only when there is nothing to check today/tomorrow a short toast (Week_NoTrips); commit on dev.
    - [2026-10-22] (@Core) VERIFY: 166 tests, tour 142/142, screenshot week-checked without the box

- [x] **T-062** Storage audit: no leftover data from updates, old shifts or connections · owner: @Core · origin: dev request 2026-10-22
  - **Log:**
    - [2026-10-22] (@Core) PROGRESS: audit: kackzug.json holds shifts (connections live inside their shift), current + next week only, check states (older than yesterday removed by DataMaintenance; removing a shift removes its states and frees its days); pending-alarms.json; alarm codes file (rewritten); updates/ dir. Leftovers found: (1) the downloaded APK (~45 MB) stayed after a successful update until the next download; (2) PendingAlarmStore.RemoveExpired was never called, so an alarm whose repeat was lost (reboot) stayed in the file and its ongoing notification stayed.
    - [2026-10-22] (@Core) DONE: module 0.2.0 UpdateDownloader.RemoveOutdated/VersionInFileName (AgentStarter feature/app-updater d601cc7, local, +6 tests, 82/82), vendored copy updated (VENDORED.md), UpdateController.RemoveOutdatedDownloads on app start; AlarmFlow.RemoveExpired (cancels their notifications) on app start and boot; commit on dev.
    - [2026-10-22] (@Core) VERIFY: KackZug 166 tests, tour 142/142, Android build ok; module 82/82 · not verified: on the phone after a real update
    - [2026-10-22] (@Dev/Bastian) VERIFY: after the in-app update to v0.4.2-dev.5: user data 475 KB, cache 442 KB (no APK left); app size 105 MB is the installed app itself (backlog B7)

## M8 / E-12: Einzelfahrten (v0.5.0, 2026-10-22) · released v0.5.0
- [2026-10-22] (@Dev/Bastian) VERIFY: "sieht alles super aus, ist ready für main und release"; main = dev = bc6c1a2, tag v0.5.0
- [x] **T-063** One-off trips: model, calendar/planner/checker, alerts without direction, auto-removal, tests · owner: @Core · spec: specs/S-one-off-trips.md, D-013 · claimed: @Core @ 2026-10-22
- [x] **T-064** Saving as "Einzelfahrt" in import and manual entry (date, name, validation) · owner: @Core · depends-on: T-063 · claimed: @Core @ 2026-10-22
- [x] **T-065** Tab "Fahrten": cards with badge, details, Navigator, delete, empty state; tour; README both languages · owner: @Core · depends-on: T-063 · claimed: @Core @ 2026-10-22
  - **Log:**
    - [2026-10-22] (@Core) DONE (T-063..T-065 by subagent, reviewed): OneOffTrip model + AppData.Trips/FindTrip/RemoveTrip, TripPlan with Id/Name/Shift?/OneOffTrip?, TripCalendar yields trips on their date, TripChecker falls back to trips, alerts without direction, DataMaintenance.Apply(data, now, zone) removes trips 60 min after planned last arrival; import target "Einzelfahrt" with date field (DateInput, dots while typing, weekday), optional name, validation (invalid/past/already over); Trips tab (cards, details, Navigator, delete, empty state, manual entry); README both languages. Commits 6094772, ca76c21, 8c75eff, 8789cd8 + fix (no past hint while typing the year).
    - [2026-10-22] (@Core) VERIFY: 207 tests (+41), tour 170/170 on the branch, 172/172 after merging T-066; screenshots trips-light/import-trip checked; emulator: Trips tab + empty state in English · not verified: saving a real shared connection as one-off trip on the phone
- [x] **T-066** Settings "Ton" and "Vollbild auf dem Sperrbildschirm" (default on) between "Alarm wiederholen" and "Bitte nicht stoeren" · owner: @Core · origin: dev request 2026-10-22 · claimed: @Core @ 2026-10-22
  - **Log:**
    - [2026-10-22] (@Core) DONE: Settings.AlarmSound/AlarmFullScreen; silent problem channels stoerung_silent_v1 / stoerung_silent_dnd_v1 (created on demand, vibration kept; channels keep their sound forever); AlarmFlow.Post picks the channel and sets the full-screen intent only when on; README both languages; branch feature/alarm-options @ faf2b17 in worktree ../KackZug-alarm-options (parallel to T-063..T-065).
    - [2026-10-22] (@Core) VERIFY: 166 tests, tour 144/144 (+2), screenshot settings-alarm (order Wiederholen, Ton, Vollbild, DND), Android build ok
    - [2026-10-22] (@Core) VERIFY: emulator: sound + full screen off, test alarm with screen off -> channel stoerung_silent_v1, mSoundNotificationKey null, vibrating, screen stayed asleep, no AlarmActivity · merged into dev (merge aa52578, string table conflict resolved by keeping both)
- [x] **T-067** Trips tab: "Jetzt pruefen (alle Fahrten)" button on top · owner: @Core · origin: dev test v0.5.0-dev.1 · claimed: @Core @ 2026-10-22
- [x] **T-068** Assign step after sharing was confusing: two tiles "Schicht" / "Einzelfahrt", then only the matching questions · owner: @Core · origin: dev test v0.5.0-dev.1 · claimed: @Core @ 2026-10-22
  - [2026-10-22] (@Core) PROGRESS: subagent builds T-067/T-068 on feature/trips-check-and-import; developer: saving one-off trips from a shared connection works ("klappt super")
  - [2026-10-22] (@Core) DONE: 92fb9e0 Trips "Jetzt pruefen (alle Fahrten)" (all trips via TripChecker, hidden when empty, progress); bc6c1a2 assign step: heading + two tiles Schicht/Einzelfahrt (choice kind style), then "Welche Schicht?" + "Hin- oder Rueckfahrt?" or date + name; picks survive switching tiles
  - [2026-10-22] (@Core) VERIFY: 207 tests, tour 185/185 (+13), screenshots import-kind-shift/oneoff/trips-check checked, Android build ok; fast-forwarded into dev (local)

## M9 / E-13: v0.5.1 (2026-10-22) · released v0.5.1
- [2026-10-22] (@Dev/Bastian) VERIFY: "Passt, neuer release auf main" after v0.5.1-dev.2 (17:08 return trip fine); main = dev = ba9c18c, tag v0.5.1
- [x] **T-069** Tutorial explains one-off trips and that they are deleted after the trip · owner: @Core · origin: dev feedback v0.5.0
  - [2026-10-22] (@Core) DONE: own tutorial page "Einzelfahrten"/"One-off trips" (train icon) after "Woche planen"; shown again after the update (tutorial per version); commit on dev
  - [2026-10-22] (@Core) VERIFY: 207 tests, tour 186/186 (+1), screenshot tutorial-trips
- [x] **T-070** "Keine Daten" for a return trip at 17:08 although bahn.de has the train · owner: @Core · origin: dev test v0.5.0 (2026-10-22 13:30)
  - [2026-10-22] (@Core) PROGRESS: live probe: Jena West board 17:08 has RE3 3962 (Jena-Goeschwitz, Jena West, Weimar), no notes; the developer's exact connection is unknown. Unknown comes from: board error/empty (reason text existed) or destination stop missing in the trip's stop list (matched only by extId, no reason shown).
  - [2026-10-22] (@Core) DONE: TripReader matches departure/destination by extId or by stop name; missing destination sets Problem "Der Halt X fehlt in den Daten von bahn.de fuer diesen Zug."; trip details list the reason per leg ("A -> B: reason") instead of the generic note; commit on dev
  - [2026-10-22] (@Core) VERIFY: 208 tests (+1), tour ok, Android build ok · not verified: the developer's 17:08 case; next step: developer opens Details of that trip with the new build and reports the reason
  - [2026-10-22] (@Dev/Bastian) VERIFY: v0.5.1-dev.1 details: "Jena West -> Erfurt Hbf: Der Halt Erfurt Hbf fehlt in den Daten von bahn.de fuer diesen Zug."; DB Navigator shows "RE3 3962 / RE3 18682" to Erfurt
  - [2026-10-22] (@Core) DONE: cause: bahn.de keeps RE3 3962 as two journeys (Jena-Goeschwitz - Jena West - Weimar, "irregulaereVerkehrstage 22. Okt bis 12. Dez 2026"; and RE3 18682 with ZE#3962 Weimar 17:24 -> Erfurt Hbf 17:40). TripReader records EndsAt/EndsAtEva/EndsAtArrival/ChangedDays when the destination is missing; BahnWebTripStatusSource looks at the board of the last stop for the same train number within 30 min, reads the destination from that journey (TripReader.ApplyContinuation); only without continuation it is PartlyCancelled "Nur bis X" / "faehrt nur bis X" with the changed days in the details; lookup errors -> no data. Likely also the cause of the first report (T-045) and T-058; T-058's "no data" interpretation was wrong for this case.
  - [2026-10-22] (@Core) VERIFY: 210 tests (+2: continuation, early end badge), tour ok, Android build ok; live run of the real check code for RE3 3962 Jena West 17:08 -> Erfurt Hbf on 2026-10-22: Found, ArrivalKnown, no problem (temporary console, deleted) · not verified: on the phone

## Backlog (not planned yet; from Vision.md, D-005 and the design system; developer decides)
- B1 (-> T-031) Plan next week (WeekSwitcher "Diese Woche / Naechste Woche") · Vision §8, D-005 deferred
- B2 (-> T-032) "Ganze Woche setzen": choose one shift for several days at once (DayRow long press) · Vision §8, design system DayRow
- B3 (-> T-037, done) Manual leg entry as fallback when a share link cannot be read · Vision §4.3, D-005 deferred
- B4 (-> T-033) Trip detail view with big time + live state ("jetzt 06:54", TimeDisplay component) · design system
- B5 Official Timetables API with the user's own key as second data source · D-002
- B6 (-> T-036, done) Optional: alarms may bypass "Bitte nicht stoeren" · Vision §7
- B7 Smaller APK (arm64 only) · open size question

## Bugs
- [ ] **T-071** Untersuchen: Einzelverbindungs Meldung · owner: @Core · priority: next · bug-investigation: ja · priority: next · origin: Board 2026-11-12
  - Bug gemeldet. Was passiert: eine Meldung für eine Einzelverbindung kam nicht an, ob das bei den gespeicherten auch passiert kann ich so nicht direkt sagen · Erwartet: meldung sollte wie geplant darauf aufmerksam machen Ursache finden und belegen, nichts beheben. Gibt es mehrere Ursachen, für jede eine Beheben-Aufgabe anlegen; sonst die folgende präzisieren (welche Datei, was ändern).
  - **DoD:** Ursache gefunden und belegt (Reproduktion oder ein Test, der fehlschlägt); Befund als Log-Zeile in dieser Aufgabe; die Beheben-Aufgabe(n) sagen konkret, was zu ändern ist.
- [ ] **T-072** Beheben: Einzelverbindungs Meldung · owner: @Core · depends-on: T-071 · bug-fix-for: T-071 · origin: Board 2026-11-12
  - Den in der Untersuchung gefundenen Fehler beheben. Was passiert: eine Meldung für eine Einzelverbindung kam nicht an, ob das bei den gespeicherten auch passiert kann ich so nicht direkt sagen · Erwartet: meldung sollte wie geplant darauf aufmerksam machen
  - **DoD:** Fehler behoben; ein Test deckt ihn ab und schlug vorher fehl; die Schritte aus der Meldung laufen ohne Fehler.
