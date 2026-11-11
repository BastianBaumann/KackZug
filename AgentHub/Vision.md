# ZugWache – Implementierungsplan für Claude Code

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).
> Read-only copy of the developer's original PLAN.md (2026-10-20). Superseded where DesignDocument.md or ADRs say so (name is KackZug, open source).

Kleine Android-App (Avalonia, C#), die einem Pendler meldet, wenn seine geplanten Zugverbindungen zur Arbeit oder zurück verspätet sind, ausfallen oder so nicht mehr existieren.

Arbeitsname: **ZugWache** (frei änderbar).

---

## 1. Ziel und Rahmen

**Kernidee:** Der Nutzer legt einmalig „Dienste“ an (z. B. Frühdienst, Spätdienst), jeweils mit einer Hin- und einer Rückfahrt. Pro Woche ordnet er jedem Tag einen Dienst oder „frei“ zu. Die App prüft die Verbindungen rechtzeitig vor Abfahrt und schlägt laut Alarm, wenn etwas nicht stimmt.

**Muss:**
- Verbindungen per „Teilen“ aus dem DB Navigator importieren
- Beim Import: neuen Dienst anlegen oder bestehenden Dienst bearbeiten, Verbindung als Hin- oder Rückfahrt speichern
- Wochenplan: pro Tag Dienst oder frei, jederzeit in der laufenden Woche änderbar
- Prüfungen zu einstellbaren Zeiten vor Abfahrt
- Auffällige, „nervige“ Benachrichtigungen bei Problemen
- Rein lokal, kein eigener Server

**Nicht-Ziele:**
- Kein Ticketkauf, keine Verbindungssuche in der App (Import nur per Teilen, plus manuelle Eingabe als Fallback)
- Keine Historie: vergangene Wochen/Prüfergebnisse werden nicht gespeichert
- Kein iOS, kein Play-Store-Release (APK wird direkt installiert)
- Keine Accounts, kein Cloud-Sync

**Leitlinie:** So klein wie möglich. Wenige Abhängigkeiten, eine JSON-Datei als Speicher, keine Datenbank.

---

## 2. Technik

| Bereich | Wahl |
|---|---|
| Sprache/Runtime | C#, aktuelles .NET LTS (zum Planungszeitpunkt .NET 10) |
| UI | Avalonia, aktuelle stabile Version, MVVM mit CommunityToolkit.Mvvm |
| Plattform | Android (Avalonia.Android), zusätzlich Desktop-Head nur für schnelle UI-Entwicklung |
| Speicher | Eine JSON-Datei (System.Text.Json), atomar geschrieben |
| HTTP | HttpClient, kein zusätzliches REST-Framework |
| Tests | xUnit, XML-Fixtures für API-Antworten |

Vor dem Start: installierte .NET-/Avalonia-Versionen prüfen und die aktuellen stabilen Versionen verwenden, nicht raten.

### Projektstruktur

```
ZugWache.sln
├─ src/
│  ├─ ZugWache.Core/        # net10.0 – Modelle, Speicher, API-Clients, Abgleich, Prüflogik, Zeitplanung (plattformneutral)
│  ├─ ZugWache.App/         # Avalonia UI (Views, ViewModels), geteilt
│  ├─ ZugWache.Android/     # Android-Head: MainActivity, Share-Intent, Alarme, Receiver, Notifications
│  └─ ZugWache.Desktop/     # Desktop-Head nur zum Entwickeln (Alarme/Notifications als No-op/Log)
└─ tests/
   └─ ZugWache.Core.Tests/
```

Plattformspezifisches nur über Interfaces in Core, implementiert im Android-Projekt:

```csharp
public interface IAlarmScheduler { void ScheduleAll(IEnumerable<PlannedCheck> checks); void CancelAll(); }
public interface IAlertNotifier  { void ShowProblem(TripAlert alert); void ShowInfo(TripStatus status); }
public interface ISecretsProvider { string ClientId { get; } string ApiKey { get; } }
```

### API-Zugangsdaten

DB-Client-Id und DB-Api-Key (DB API Marketplace, Timetables API abonniert) kommen aus einer **nicht eingecheckten** Datei (z. B. `secrets.json`, in `.gitignore`), die beim Build als Asset oder generierte Konstante eingebunden wird. Eine `secrets.example.json` mit Platzhaltern einchecken. Für eine private APK ist das ausreichend.

---

## 3. Datenmodell

Langfristig gespeichert werden nur **Dienste mit Verbindungen**, **Einstellungen** und der **Wochenplan** (aktuelle Woche, optional nächste Woche). Alles Ältere wird beim Laden verworfen.

```csharp
public sealed record AppData(
    int SchemaVersion,
    List<Shift> Shifts,
    List<WeekPlan> Weeks,          // max. aktuelle + nächste Woche
    Settings Settings,
    List<CheckState> CheckStates); // nur kurzlebig, siehe unten

public sealed record Shift(
    Guid Id,
    string Name,                    // "Frühdienst"
    string? ColorHex,
    SavedConnection? Outbound,      // Hinfahrt
    SavedConnection? Return);       // Rückfahrt

public sealed record SavedConnection(
    List<Leg> Legs,                 // 1..n Teilstrecken (Umstiege)
    string? SourceVbid,             // aus dem geteilten Link, nur zur Info
    DateTimeOffset ImportedAt);

public sealed record Leg(
    Station From, Station To,
    TimeOnly PlannedDeparture,
    TimeOnly PlannedArrival,
    int ArrivalDayOffset,           // 1, falls Ankunft nach Mitternacht
    string Category,                // "RE", "RB", "ICE", "S"
    string? Line,                   // "1", "S3"
    string? TrainNumber,            // nur Hilfsmerkmal, nicht primärer Schlüssel
    string? PlannedPlatform);

public sealed record Station(string Name, string? EvaNumber);

public sealed record WeekPlan(DateOnly WeekStart /* Montag */, Dictionary<DayOfWeek, DayPlan> Days);

public sealed record DayPlan(
    Guid? ShiftId,                  // null = frei
    bool OutboundEnabled = true,    // einzelne Fahrt an diesem Tag abschalten
    bool ReturnEnabled = true);

public sealed record Settings(
    List<int> LeadTimesMinutes,     // z. B. [90, 45, 15]
    TimeOnly? EveningCheckTime,     // z. B. 20:00 am Vortag, null = aus
    bool EveningOkNotification,     // auch „alles planmäßig“ melden
    int DelayThresholdMinutes,      // ab wann eine Verspätung gemeldet wird (Standard 5)
    int MinTransferMinutes,         // Mindestumstiegszeit (Standard 4)
    bool InsistentAlerts,           // Ton wiederholt sich bis zur Reaktion
    bool FullScreenAlerts,          // Vollbild-Alarm bei Ausfall
    NewWeekMode NewWeekMode);       // CopyPrevious | Empty

public sealed record CheckState(   // verhindert doppelte Meldungen
    DateOnly Date, Guid ShiftId, TripDirection Direction,
    string LastStatusHash, DateTimeOffset LastNotifiedAt);
```

**Regeln:**
- Wochenwechsel: Beim Laden und beim täglichen Wartungsalarm prüfen, ob eine neue Woche begonnen hat. Neue Woche gemäß `NewWeekMode` anlegen (Standard: Vorwoche kopieren), vergangene Wochen löschen.
- `CheckStates` älter als gestern löschen.
- Speichern: in temporäre Datei schreiben, dann umbenennen. `SchemaVersion` für spätere Migrationen.
- Ort: App-Datenverzeichnis (`Environment.SpecialFolder.LocalApplicationData` bzw. Android `FilesDir`).

---

## 4. Import per „Teilen“ aus dem DB Navigator

### 4.1 Android-Seite

- `MainActivity` mit Intent-Filter für `android.intent.action.SEND`, MIME-Typ `text/plain`.
- `LaunchMode = SingleTop`, Intents sowohl in `OnCreate` als auch in `OnNewIntent` verarbeiten.
- Geteilten Text an die App-Schicht übergeben (z. B. über einen Service in Core/App, den die Activity aufruft).

### 4.2 Link auswerten

- Geteilter Text enthält einen Link der Form `https://www.bahn.de/buchung/start?vbid=<id>`, eventuell mit weiterem Text drumherum. Mit Regex die URL und die `vbid` extrahieren.
- Auflösung über `IConnectionResolver`:

```csharp
public interface IConnectionResolver
{
    Task<ResolvedConnection> ResolveAsync(string sharedText, CancellationToken ct);
}
```

- Implementierung `BahnDeVbidResolver`: löst die vbid über die **inoffizielle bahn.de-Web-API** auf und mappt das Ergebnis auf Legs (Stationen inkl. EVA-Nummer, geplante Zeiten, Kategorie, Linie, Zugnummer, Gleis).
  - Der genaue Endpunkt und das Antwortformat sind **nicht offiziell dokumentiert**. Vor der Implementierung recherchieren und mit einem echten Link verifizieren. Referenz: Open-Source-Projekt „Besser Bahn“ (github.com/chuk-development/Besser-Bahn), das solche Links ebenfalls verarbeitet.
  - Antwort als Fixture in den Tests ablegen.
  - Fußwege und Nicht-Zug-Abschnitte ignorieren; nur echte Fahrten werden Legs.
- Fehlerfall (Endpunkt geändert, offline): klare Meldung im Import-Dialog und Angebot der manuellen Eingabe.

### 4.3 Manuelle Eingabe (Fallback)

Pro Leg: Startbahnhof, Zielbahnhof (Suche über Timetables `/station/{pattern}` zur EVA-Ermittlung), Abfahrt, Ankunft, Kategorie, Linie. Bewusst schlicht.

### 4.4 Import-Dialog (UI-Ablauf)

1. Zusammenfassung der erkannten Verbindung: „Erfurt Hbf 06:42 → Weimar 06:58, RE 1“ (bei Umstiegen alle Teilstrecken).
2. Auswahl: **Neuen Dienst anlegen** (Name eingeben) oder **Bestehenden Dienst bearbeiten** (Liste).
3. Auswahl: **Hinfahrt** oder **Rückfahrt**.
4. Ist der Platz schon belegt: „Vorhandene Hinfahrt ersetzen?“ mit Vorher/Nachher.
5. Speichern → Alarme neu planen → zurück zur Dienstansicht.

Hinweis an den Nutzer: Gespeichert wird das Muster (Bahnhöfe, Uhrzeiten, Linie), nicht das Datum der geteilten Verbindung.

---

## 5. Live-Daten: DB Timetables API

Basis: `https://apis.deutschebahn.com/db-api-marketplace/apis/timetables/v1`
Header: `DB-Client-Id`, `DB-Api-Key`, `Accept: application/xml`

| Endpunkt | Zweck |
|---|---|
| `GET /station/{pattern}` | Bahnhof → EVA-Nummer (nur, falls beim Import keine EVA mitkam) |
| `GET /plan/{eva}/{yyMMdd}/{HH}` | Geplante Halte einer Stunde |
| `GET /fchg/{eva}` | Alle bekannten Änderungen (Verspätungen, Gleise, Ausfälle) |
| `GET /rchg/{eva}` | Nur kürzliche Änderungen (für Folgeprüfungen kurz vor Abfahrt) |

**Wichtig:**
- Daten nur für ca. die nächsten **18 Stunden**.
- XML mit Kurzattributen: `tl` (Zug: `c` Kategorie, `n` Nummer), `dp`/`ar` (Abfahrt/Ankunft: `pt` geplante Zeit, `ct` geänderte Zeit, `pp`/`cp` Gleis, `l` Linie, `cs` Status, z. B. `c` = storniert), Zeiten im Format `yyMMddHHmm`, Ortszeit Europe/Berlin.
- Plan und Änderungen über die Halt-ID (`s id`) zusammenführen.
- Die genaue Attributbedeutung aus der offiziellen OpenAPI-Spezifikation des Marketplace übernehmen, nicht raten.
- Rate-Limits des gewählten Plans im Marketplace nachsehen. Die App braucht nur wenige Aufrufe; trotzdem pro Prüfung Ergebnisse je Bahnhof cachen und nicht parallel feuern.

### 5.1 Abgleich (Matching)

Für jedes Leg am Zieltag:
1. Plan der Stunde der geplanten Abfahrt am Startbahnhof laden (bei Abfahrt kurz nach voller Stunde auch die vorherige Stunde, wegen Randfällen).
2. Kandidaten: Halte mit `dp`, deren `pt` zeitlich exakt passt, und gleiche Kategorie; Linie als zweites, Zugnummer als drittes Kriterium.
3. Kein Kandidat → Status **NichtGefunden** („Verbindung fährt so nicht mehr“).
4. Änderungen (`fchg`/`rchg`) auf den gefundenen Halt anwenden.
5. Für Umstiege zusätzlich die **Ankunft** am Umstiegsbahnhof aus dessen Plan/Änderungen ermitteln.

### 5.2 Status pro Leg und Fahrt

```csharp
public enum LegStatus { Planmaessig, Verspaetet, Gleiswechsel, Ausfall, TeilAusfall, NichtGefunden, Unbekannt }
```

- **Ausfall:** Abfahrt am Startbahnhof storniert.
- **TeilAusfall:** Zug fährt, aber Zielhalt bzw. Umstiegshalt fällt aus (Ankunft storniert oder Halt fehlt).
- **Verspätet:** `ct - pt >= DelayThresholdMinutes`.
- **Anschluss gefährdet:** tatsächliche Ankunft Leg n + `MinTransferMinutes` > tatsächliche Abfahrt Leg n+1.
- **Unbekannt:** API nicht erreichbar oder Zeitpunkt außerhalb des 18-h-Fensters. Kein Alarm, aber Wiederholung einplanen; wenn kurz vor Abfahrt immer noch unbekannt, Info-Hinweis „Konnte nicht prüfen“.

Gesamtstatus einer Fahrt = schlimmster Leg-Status. Die Logik ist reine Core-Logik und vollständig unit-testbar.

---

## 6. Zeitplanung der Prüfungen (Android)

Die App läuft nicht dauerhaft. Prüfungen werden als **exakte Alarme** zu bekannten Zeitpunkten geplant.

### 6.1 Welche Prüfungen

Für jede aktive Fahrt (Tag mit Dienst, Fahrt aktiviert) der aktuellen und ggf. nächsten Woche, in den **nächsten 48 Stunden**:
- **Vorab-Check:** am Vortag zur `EveningCheckTime`. Liegt die Abfahrt dann mehr als 17 h entfernt (typisch bei Rückfahrten am Abend), wird der Vorab-Check auf `Abfahrt − 17 h` verschoben, aber nicht in die Nacht (00:00–06:00 → 06:00).
- **Vorlauf-Checks:** zu jedem Wert aus `LeadTimesMinutes` vor der ersten Abfahrt.
- **Folge-Checks:** Wurde ein Problem oder eine Verspätung erkannt, bis zur Abfahrt alle 5 Minuten zusätzlich prüfen (über `rchg`), solange sich etwas ändern kann.
- **Wartungsalarm:** täglich ca. 00:10: Wochenwechsel, Aufräumen, alle Alarme der nächsten 48 h neu planen.

Die Berechnung (`CheckPlanner`) liegt in Core und bekommt „jetzt“ als Parameter, damit sie testbar ist. Sommer-/Winterzeitwechsel explizit testen.

### 6.2 Android-Umsetzung

- `AlarmManager.SetExactAndAllowWhileIdle` mit eindeutigen `PendingIntent`-Request-Codes pro Prüfung.
- Berechtigungen: `SCHEDULE_EXACT_ALARM` (Android 12) und `USE_EXACT_ALARM` (Android 13+, für direkt installierte APK unproblematisch). Beim Start prüfen, ob exakte Alarme erlaubt sind, sonst in die Systemeinstellung führen.
- `CheckAlarmReceiver`: nutzt `GoAsync()`, führt die Prüfung mit hartem Timeout (ca. 8 s) aus, zeigt ggf. Benachrichtigung, plant Folge-Checks. Bei Timeout/Netzfehler: Wiederholung in 2 Minuten.
- `BootReceiver` für `BOOT_COMPLETED`, `MY_PACKAGE_REPLACED`, `TIME_SET`, `TIMEZONE_CHANGED`: alle Alarme neu planen.
- Nach jeder Änderung an Diensten, Wochenplan oder Einstellungen: alle Alarme neu planen (einfach: alles stornieren, alles neu setzen).
- Beim ersten Start Ausnahme von der Akkuoptimierung anfragen (`REQUEST_IGNORE_BATTERY_OPTIMIZATIONS`) und kurz erklären, warum.

---

## 7. Benachrichtigungen („groß und nervig“)

Zwei Kanäle:

| Kanal | Wofür | Verhalten |
|---|---|---|
| **Störung** | Ausfall, TeilAusfall, NichtGefunden, Verspätung ≥ Schwelle, Anschluss gefährdet, Gleiswechsel | `IMPORTANCE_HIGH` (Heads-up), eigener auffälliger Ton, langes Vibrationsmuster, Kategorie Alarm, optional `FLAG_INSISTENT` (Ton wiederholt sich bis zur Reaktion), optional Vollbild-Intent bei Ausfall/NichtGefunden |
| **Info** | „Morgen 06:42 RE 1 fährt planmäßig“, „Konnte nicht prüfen“ | normale Priorität, kurz |

- Text konkret: „🚫 FRÜHDIENST HINFAHRT: RE 1 06:42 Erfurt Hbf → Weimar FÄLLT AUS“, darunter Details (Verspätung in Minuten, neues Gleis, betroffene Teilstrecke).
- Aktionen in der Benachrichtigung: „Gesehen“ (stoppt Wiederholungen für diese Fahrt/diesen Status) und „DB Navigator öffnen“.
- Pro Fahrt/Tag nur erneut alarmieren, wenn sich der Status ändert (`CheckState.LastStatusHash`).
- Berechtigung `POST_NOTIFICATIONS` (Android 13+) beim ersten Start anfragen. Vollbild: `USE_FULL_SCREEN_INTENT`; auf neueren Android-Versionen ggf. Nutzerfreigabe nötig, Einstellungsseite verlinken.
- Kein Umgehen von „Bitte nicht stören“ im MVP; als mögliche spätere Option notieren.

---

## 8. Oberfläche

Bewusst wenige Bildschirme, große Bedienelemente.

1. **Woche (Startseite)**
   - Umschalter aktuelle Woche / nächste Woche.
   - Liste Mo–So: Tag, Datum, Dienst-Auswahl (Dienste + „frei“), kleine Schalter für Hin/Rück, letzter bekannter Status (grün/gelb/rot/grau).
   - Vergangene Tage der laufenden Woche ausgegraut, nicht editierbar; heutige und kommende Tage jederzeit änderbar.
   - „Ganze Woche setzen“: einen Dienst für mehrere markierte Tage auf einmal.
2. **Dienste**
   - Liste der Dienste mit Hin- und Rückfahrt-Zusammenfassung.
   - Bearbeiten: Name, Farbe, Hin/Rück anzeigen, ersetzen (Hinweis „Verbindung im DB Navigator teilen“), manuell eingeben, entfernen, Dienst löschen (mit Rückfrage; betroffene Tage werden „frei“).
3. **Import-Dialog** (siehe 4.4), öffnet sich beim Teilen automatisch.
4. **Einstellungen**
   - Vorlaufzeiten (Mehrfachauswahl, z. B. 120/90/60/45/30/15 min, eigener Wert möglich)
   - Vorab-Check-Uhrzeit, Info bei „planmäßig“ an/aus
   - Verspätungsschwelle, Mindestumstiegszeit
   - Insistierender Ton an/aus, Vollbild-Alarm an/aus
   - Neue Woche: Vorwoche kopieren / leer
   - Berechtigungs-Check mit Status (Benachrichtigungen, exakte Alarme, Akkuoptimierung)
   - **Test-Alarm** auslösen
5. Optional im MVP, sonst später: „Jetzt prüfen“-Knopf auf der Startseite.

---

## 9. Umsetzung in Etappen

Jede Etappe endet mit grünem Build und grünen Tests. Kleine, nachvollziehbare Commits.

1. **Gerüst**: Solution, Projekte, Avalonia-App läuft auf Desktop und Android mit leerer Startseite. `.gitignore`, `secrets.example.json`, README mit Build-Anleitung.
   *Fertig, wenn:* APK baut und startet auf einem Gerät/Emulator.
2. **Core-Modelle und Speicher**: Datenmodell, JSON-Speicher (atomar), Wochenwechsel, Aufräumen.
   *Fertig, wenn:* Tests für Laden/Speichern, Wochenwechsel (Kopieren/Leer), Löschen alter Daten grün.
3. **Timetables-Client und Abgleich**: HTTP-Client, XML-Parser, Matching, Statusermittlung inkl. Umstieg.
   *Fertig, wenn:* Tests mit XML-Fixtures für planmäßig, verspätet, storniert, Gleiswechsel, Zug fehlt, Anschluss gefährdet grün; ein manueller Testlauf gegen die echte API klappt.
4. **Prüfzeitplanung**: `CheckPlanner` inkl. Vorab-Check-Regel, Vorlaufzeiten, Folge-Checks.
   *Fertig, wenn:* Tests inkl. Mitternacht und Zeitumstellung grün.
5. **Import**: Link-Erkennung, `BahnDeVbidResolver` (nach Recherche des Endpunkts), manuelle Eingabe.
   *Fertig, wenn:* echter geteilter Link aus dem DB Navigator wird korrekt in Legs übersetzt; Fixture-Test vorhanden.
6. **UI**: Woche, Dienste, Import-Dialog, Einstellungen.
   *Fertig, wenn:* kompletter Ablauf Teilen → Dienst anlegen → Woche planen → Änderung in laufender Woche funktioniert.
7. **Android-Hintergrund und Benachrichtigungen**: Alarme, Receiver, Boot-Receiver, Kanäle, insistente/Vollbild-Alarme, Berechtigungsdialoge, Test-Alarm.
   *Fertig, wenn:* Test-Alarm ist laut und auffällig; eine geplante Prüfung löst bei gesperrtem Bildschirm aus; nach Neustart sind Alarme wieder geplant.
8. **Feinschliff**: Fehlermeldungen, leere Zustände, App-Icon, Release-APK signieren, README für den Endnutzer (Installation, Berechtigungen, Akkuoptimierung).

---

## 10. Tests

- **Unit (Core):** Parser, Matching, Status, Anschlusslogik, CheckPlanner (Zeitumstellung, Mitternacht, 18-h-Fenster), Wochenwechsel, Speicher-Roundtrip, Link-Extraktion.
- **Fixtures:** echte, anonymisierte API-Antworten (Timetables XML, bahn.de-Antwort zur vbid) unter `tests/.../Fixtures`.
- **Manuell auf dem Gerät:** Teilen aus dem DB Navigator, Alarm bei gesperrtem Gerät und im Doze-Modus (`adb shell dumpsys deviceidle force-idle`), Neustart, Berechtigungen verweigert.

---

## 11. Risiken und offene Punkte

- **Inoffizielle bahn.de-API** für die vbid-Auflösung kann sich ändern → hinter Interface kapseln, Fixture-Test, manuelle Eingabe als Fallback.
- **18-Stunden-Fenster** der Timetables API → längerfristige Änderungen (Baustellen) werden erst am Vortag sichtbar. Für das MVP akzeptiert.
- **Schienenersatzverkehr** taucht in der Timetables API nicht als Zug auf → wird als Ausfall/NichtGefunden gemeldet; Text entsprechend formulieren („fährt nicht wie geplant, ggf. Ersatzverkehr – im DB Navigator prüfen“).
- **Hersteller-Akkusparmodi** (Xiaomi, Huawei, Samsung) können Alarme trotzdem verzögern → im README und in den Einstellungen auf die Herstellereinstellungen hinweisen.
- **Fahrplanwechsel** (Mitte Dezember): Matching schlägt dann gezielt mit „NichtGefunden“ an – das ist gewollt und fordert zum erneuten Teilen auf.

---

## 12. Arbeitsweise für Claude Code

- Plan etappenweise abarbeiten, nach jeder Etappe kurz zusammenfassen, was fertig ist und was offen bleibt.
- Keine zusätzlichen NuGet-Pakete ohne klaren Grund (Ziel: Avalonia, CommunityToolkit.Mvvm, xUnit – mehr nicht).
- Unklare API-Details (Timetables-Attribute, bahn.de-Endpunkt) recherchieren und verifizieren statt annehmen; Annahmen im Code kommentieren.
- Secrets niemals committen.
- Bei Abweichungen vom Plan (z. B. Avalonia-Einschränkung auf Android): kurz begründen und Alternative vorschlagen.
