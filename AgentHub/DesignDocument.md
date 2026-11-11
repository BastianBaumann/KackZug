# Design Document: KackZug

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Status:** APPROVED (2026-10-20) · **Version:** v3 · **Last updated:** 2026-10-20
- **Version history:** v1 (2026-10-20) initial, based on Vision.md + D-001..D-008 · v2 (2026-10-20) live API findings (§6.1) · v3 (2026-10-20) design system (D-009, D-010), full-screen alarm (T-024), PlannedOnly status (T-027)

## 1. Problem & goals
Pendler mit Schichtdienst wollen wissen, ob ihre feste Hin- und Rueckfahrt heute noch so faehrt.
KackZug speichert Schichten mit je einer Hin- und Rueckfahrt (Import per "Teilen" aus dem DB Navigator),
ordnet Schichten den Wochentagen zu und prueft die Verbindungen vom Handy aus zu festen Zeiten vor
Abfahrt. Bei Ausfall, Verspaetung, Gleiswechsel oder gefaehrdetem Anschluss kommt eine laute
Benachrichtigung. Rein lokal, kein Server, Open Source (GPL-3.0-or-later).

## 2. Scope (in / out / MVP)
**MVP (D-005):**
- Schichten anlegen/bearbeiten/loeschen (Name, Farbe, Hinfahrt, Rueckfahrt)
- Import per Android-Teilen (Text mit `vbid`-Link): neue Schicht oder bestehende, Hin oder Rueck, Ersetzen mit Vorher/Nachher
- Wochenplan der laufenden Woche: pro Tag Schicht oder frei, Hin/Rueck einzeln abschaltbar; neue Woche kopiert Vorwoche (oder leer, Einstellung)
- Feste Pruefzeitpunkte: Abend-Check am Vortag + Vorlaufzeiten (Standard 90/45/15 min), taeglicher Wartungsalarm 00:10
- Benachrichtigungen: Kanal "Stoerung" (laut, Heads-up, langes Vibrieren, optional insistierend) und "Info" (planmaessig / konnte nicht pruefen)
- "Jetzt pruefen" und "Test-Alarm" Knoepfe
- Einstellungen inkl. Berechtigungsstatus, zwei Farben + Hell/Dunkel
- Update-Hinweis aus GitHub Releases (D-004)
- Kurzes Tutorial (wenige Seiten: Teilen aus DB Navigator, Schicht, Wochenplan, Benachrichtigungen/Berechtigungen) beim ersten Start und nach jedem Update; jederzeit in den Einstellungen erneut aufrufbar (Developer-Wunsch 2026-10-20, am Ende des MVP)
- Eigenes Logo (SVG + Android-Vector-Icon), README (Deutsch), LICENSE, CI + Release-Pipeline

**Nicht im MVP:** Folge-Checks (gestrichen), manuelle Eingabe von Teilstrecken, Vollbild-Alarm, naechste
Woche planen, Timetables-API mit eigenem Schluessel, iOS, Play Store, Historie.

## 3. Architecture
```
 DB Navigator --share text--> MainActivity (Android head)
                                   |
                                   v
 +------------------ KackZug.App (Avalonia, MVVM) -------------------+
 | WeekView  ShiftsView  ImportView  SettingsView   UpdateBanner      |
 +------------------------------|------------------------------------+
                                v
 +------------------ KackZug.Core (net10.0, no UI) ------------------+
 | ShareLinkParser -> IConnectionResolver (BahnWebConnectionResolver)|
 | AppDataStore (JSON) . WeekRollover . CheckPlanner                  |
 | TripChecker -> ITripStatusSource (BahnWebTripStatusSource)        |
 | StatusEvaluator . AlertComposer . UpdateChecker                    |
 | Interfaces: IAlarmScheduler IAlertNotifier IClock IAppPaths        |
 +------------------------------|------------------------------------+
                                v
 KackZug.Android: AlarmScheduler (AlarmManager), CheckAlarmReceiver,
 BootReceiver, AlertNotifier (channels), permissions helpers
 KackZug.Desktop: dev head, scheduler/notifier log only
        HTTP (phone direct): www.bahn.de/web/api/*  ·  api.github.com (update check)
```

## 4. Components & responsibilities (all owned by @Core)
| Component | Path |
|---|---|
| Models, storage, rollover, planner, matching, status, API clients | `src/KackZug.Core/` |
| Views, ViewModels, theming, logo geometry | `src/KackZug.App/` |
| Android head: activity, share intent, alarms, receivers, notifications, vector icon | `src/KackZug.Android/` |
| Desktop dev head | `src/KackZug.Desktop/` |
| Unit tests + hand-made fixtures | `tests/KackZug.Core.Tests/` |
| Logo source | `assets/logo/kackzug.svg` |
| CI / release | `.github/workflows/ci.yml`, `.github/workflows/release.yml` |
| Docs | `README.md` (Deutsch), `LICENSE` (GPL-3.0) |

## 5. Data model (one JSON file, atomic write: temp file + rename, in Android `FilesDir`)
```
AppData      { SchemaVersion, Shifts[], CurrentWeek: WeekPlan?, Settings, CheckStates[], LastUpdateCheck? }
Shift        { Id, Name, ColorHex, Outbound: SavedConnection?, Return: SavedConnection? }
SavedConnection { Legs[], ImportedAt }
Leg          { From: Station, To: Station, PlannedDeparture: TimeOnly, PlannedArrival: TimeOnly,
               ArrivalDayOffset, Category ("RE","ICE"), Line?, TrainNumber?, PlannedPlatform? }
Station      { Name, EvaNumber }
WeekPlan     { WeekStart (Montag), Days: Dictionary<DayOfWeek, DayPlan> }
DayPlan      { ShiftId?, OutboundEnabled, ReturnEnabled }
Settings     { LeadTimesMinutes [90,45,15], EveningCheckTime 20:00?, EveningOkNotification,
               DelayThresholdMinutes 5, MinTransferMinutes 4, InsistentAlerts, NewWeekMode (CopyPrevious|Empty),
               AccentColor, SecondaryColor, ThemeMode (System|Light|Dark), UpdateCheckEnabled,
               TutorialSeenVersion? }   // Tutorial zeigen, wenn null oder != App-Version
CheckState   { Date, ShiftId, Direction, LastStatusHash, LastNotifiedAt }   // nur heute/gestern
```
Gespeichert wird das Muster (Bahnhoefe, Uhrzeiten, Zug), nicht das Datum der geteilten Verbindung.
Zeiten sind Ortszeit Europe/Berlin.

## 6. Interfaces & contracts
```csharp
interface IConnectionResolver { Task<SavedConnection> ResolveAsync(string sharedText, CancellationToken cancellationToken); }
interface ITripStatusSource   { Task<LegObservation> ObserveAsync(Leg leg, DateOnly travelDate, CancellationToken cancellationToken); }
interface IAlarmScheduler     { void ScheduleAll(IReadOnlyList<PlannedCheck> checks); void CancelAll(); }
interface IAlertNotifier      { void ShowProblem(TripAlert alert); void ShowInfo(TripAlert alert); }
interface IClock              { DateTimeOffset Now { get; } }
interface IAppPaths           { string DataDirectory { get; } }
```
**bahn.de (inoffiziell, D-002), Stand der Recherche 2026-10-20, Host `https://www.bahn.de/web/api`,
Header `Accept: application/json`, `Accept-Language: de-DE`, Browser-User-Agent (siehe 6.1):**
- Import: `GET /angebote/verbindung/{vbid}` -> `hinfahrtRecon`; dann `POST /angebote/recon` mit `ctxRecon`
  -> `verbindungen[0].verbindungsAbschnitte[]` (Fusswege `typ WALK` ignorieren). Fallback ohne zweiten
  Aufruf: Recon-String offline zerlegen (Bahnhoefe, EVA, Zeiten, Zug; ohne Gleis).
- Pruefung pro Leg: `GET /reiseloesung/abfahrten?ortExtId={eva}&datum={yyyy-MM-dd}&zeit={HH:mm:ss}`
  -> Kandidat per geplanter Zeit + Kategorie, dann Linie, dann Zugnummer -> `journeyId`;
  dann `GET /reiseloesung/fahrt?journeyId=...` -> Ab-Halt und Ziel-Halt: Soll/Echtzeit, Gleis/ezGleis,
  `cancelled`, Ausfall-Notizen ("Halt entfaellt", "faellt aus"). Beide Zeitformate (verschachtelt und flach) parsen.
- Hoechstens ein Aufruf gleichzeitig, Ergebnisse je Bahnhof pro Pruefung cachen, bei 403/429 kein Alarm,
  sondern Status Unbekannt.

**Status:** `LegStatus { Planmaessig, Verspaetet, Gleiswechsel, Ausfall, TeilAusfall, NichtGefunden, Unbekannt }`;
Anschluss gefaehrdet, wenn Ankunft(n) + MinTransferMinutes > Abfahrt(n+1). Fahrtstatus = schlimmster Leg-Status.
Neue Meldung nur bei geaendertem Status-Hash (CheckState).

**CheckPlanner** (rein, "jetzt" als Parameter): fuer jede aktive Fahrt in den naechsten 48 h:
Abend-Check am Vortag zur EveningCheckTime (nur wenn in der Zukunft), Vorlauf-Checks je LeadTime vor
erster Abfahrt, plus Wartungsalarm 00:10. Eindeutige Request-Codes pro Check. Sommer-/Winterzeit getestet.

**Update-Check:** hoechstens 1x taeglich beim App-Start `GET https://api.github.com/repos/BastianBaumann/KackZug/releases/latest`,
Tag `vX.Y.Z` mit App-Version vergleichen, Banner "Neue Version verfuegbar" oeffnet `html_url` im Browser.

### 6.1 Live findings (2026-10-20, real shared links, samples in specs/samples/)
- Shared text from DB Navigator can be English and uses `https://int.bahn.de/en/buchung/start?vbid=<uuid>`:
  accept any `*.bahn.de` host and language path; parse only the vbid.
- A self-identifying User-Agent ("KackZug/...") gets **403 Access Denied** (bot protection). A mobile
  browser User-Agent works. So the client sends a browser-like UA (honest-UA rule dropped).
- Responses may be gzip-compressed even without asking: HttpClient with automatic decompression.
- `POST /angebote/recon` returns 201 with live data per leg: `verbindungsAbschnitte[].halte[0].abfahrt{sollzeit,echtzeit}`,
  `halte[-1].ankunft{sollzeit,echtzeit}`, `gleis`, `originCancelled`, `destinationCancelled`, `risNotizen`, `himMeldungen`.
  Walks: `verkehrsmittel.typ == "WALK"` (keep their duration for the transfer check).
  Display name: `verkehrsmittel.mittelText` ("RE1", "RB21", "STR 3"); `kategorie` is internal ("NRE", "DRB", "STR");
  `nummer` is the train number.
- Trams/buses are part of the connection; they have no `echtzeit` (planned only). They are saved and
  checked; no realtime -> treated as planmaessig unless cancelled.
- One test with the date shifted inside `ctxRecon` was answered with `{"status":"ERROR","code":"OPS_BLOCKED"}`
  (after ~6 requests in 2 minutes). Unclear whether rate limit or the shifted recon. Retest later with
  a single request. Until clarified, the status check uses board (`/reiseloesung/abfahrten`) + `/reiseloesung/fahrt`.
- Consequence: very few requests (one check run = at most ~2 requests per rail leg), never retry in a loop,
  `OPS_BLOCKED`/403/429 -> Unbekannt + back off for the rest of the day.

## 7. Tech stack & rationale
.NET 10, Avalonia 12.x (aktuellste Patch-Version bei Projektanlage, in Dependencies.md), CommunityToolkit.Mvvm,
System.Text.Json, HttpClient, xUnit (D-007). Keine weiteren Pakete. Android: minSdk 26, targetSdk 36,
Paket-ID `io.github.bastianbaumann.kackzug` (nie aendern, D-004). Logo, Symbole, Farben und Komponenten aus dem Design System "ZugKacke" (D-009, Marke Orange D-010);
tools/design/build_assets.py erzeugt Android-Icons und Avalonia-Ressourcen aus assets/. Schrift Inter.

## 8. Non-functional requirements
- Datenschutz: keine Telemetrie, keine Konten, Daten nur auf dem Geraet; README nennt, dass bahn.de und GitHub die IP sehen.
- Zuverlaessigkeit: exakte Alarme (`SetExactAndAllowWhileIdle`), Boot-/Update-/Zeitzonen-Receiver, Netzfehler -> 1 Wiederholung nach 2 min.
- Open Source: nur eigene Assets, GPL-3.0-Header nicht pro Datei (LICENSE genuegt), keine DB-Logos/Screenshots, Hinweis "inoffiziell, nicht mit der Deutschen Bahn verbunden".
- Secrets: keine im Repo. Release-Keystore nur als GitHub-Secrets (Namen: `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD`).

## 9. Risks & mitigations
- bahn.de-API aendert sich/sperrt -> Interfaces, Fixture-Tests, klare Fehlermeldung, spaeter Timetables-Option.
- Hersteller-Akkusparmodi verzoegern Alarme -> Akkuoptimierung-Ausnahme anfragen, README-Hinweis.
- Schienenersatzverkehr / Fahrplanwechsel -> NichtGefunden mit Text "faehrt nicht wie geplant, im DB Navigator pruefen / neu teilen".
- Android-Plattform 36 nicht installiert (nur 34/35) -> beim ersten Build nachinstallieren (Setup.md).
- Verlorener Keystore -> Nutzer muessen neu installieren; Developer sichert ihn offline.

## 10. Current code base
Keine (neues Projekt).

## 11. Changes after v1 (2026-10-20)
- Status `PlannedOnly`: trip found in the timetable but no realtime yet -> grey "Laut Fahrplan", info on the evening check; empty board -> Unknown (T-027).
- Problems open a full-screen alarm (AlarmActivity) that only "Gesehen"/"DB Navigator oeffnen" ends; ongoing notification re-posted on swipe; repeats every 5 min until departure (Core/Alarms, T-024).
- Test alarm is a real AlarmManager alarm (works with the app closed).
- Look follows the design system (T-023).

