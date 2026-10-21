<p align="center">
  <img src="assets/logo/kackzug-app-icon.svg" alt="KackZug Logo" width="160">
</p>

# KackZug

**Deutsch** · [English](README.md)

KackZug ist eine kleine Android-App für alle, die im Schichtdienst mit der Bahn zur Arbeit fahren. Du
speicherst deine Schichten mit Hin- und Rückfahrt, legst fest, an welchem Tag du welche Schicht hast,
und das Handy prüft vor der Abfahrt selbst, ob deine Verbindung noch so fährt. Fällt ein Zug aus, ist
er verspätet, wechselt das Gleis oder wird der Anschluss knapp, meldet sich KackZug laut und deutlich.

## Was die App kann

- **Verbindungen per Teilen übernehmen:** Im DB Navigator eine Verbindung suchen, auf „Teilen“ tippen
  und KackZug auswählen. Die Verbindung landet mit allen Teilstrecken (auch Fußwege und Straßenbahn)
  als Hin- oder Rückfahrt in einer Schicht. Klappt das Teilen nicht, lässt sich die Verbindung auch
  von Hand eingeben: Bahnhöfe suchen, Zeiten und Linie eintragen.
- **Woche planen:** Jeder Tag bekommt eine Schicht oder bleibt frei. Hin- und Rückfahrt lassen sich
  pro Tag einzeln abschalten. Die nächste Woche kannst du schon vorher planen, und mit „Mehrere Tage
  setzen“ (oder langem Druck auf einen Tag) bekommen gleich mehrere Tage dieselbe Schicht. Eine neue
  Woche übernimmt auf Wunsch den Plan der Vorwoche.
- **Details zu jeder Fahrt:** große Abfahrtszeit, aktueller Stand jeder Teilstrecke (Verspätung, neues
  Gleis, Ausfall) und wie viel Puffer beim Umsteigen bleibt. Der Knopf „Im DB Navigator öffnen“ öffnet
  die Verbindung für genau diesen Tag im DB Navigator. Ist der DB Navigator nicht installiert, öffnet
  er die Verbindung stattdessen auf bahn.de im Browser und sagt das auch.
- **Prüfen zu festen Zeiten:** am Abend vorher und kurz vor der Abfahrt, standardmäßig 90, 45 und
  15 Minuten vorher. Die Zeiten sind einstellbar.
- **Laute Meldungen bei Störungen:** Ausfall, Teilausfall, Verspätung ab einer einstellbaren Schwelle,
  Gleiswechsel, gefährdeter Anschluss oder eine Fahrt, die es so nicht mehr gibt. Es öffnet sich ein
  Vollbild-Alarm, auch auf dem Sperrbildschirm. Er klingelt und kommt alle fünf Minuten wieder, bis du
  „Gesehen“ tippst. Dafür hat KackZug einen eigenen, schrillen Ton. Auf Wunsch klingelt er auch bei
  „Bitte nicht stören“.
- **Farben, die man sofort versteht:** Meldungen, dass alles pünktlich ist, sind grün und kommen ohne Ton, Probleme sind rot und laut.
- **Ehrliche Anzeige:** bahn.de hat den Fahrplan schon Tage vorher, Echtzeitdaten aber erst kurz vor der
  Abfahrt. Solange es keine Echtzeit gibt, zeigt KackZug „Laut Fahrplan“ statt „Planmäßig“.
- **„Jetzt prüfen“** für heute und morgen, ein **Test-Alarm** und eine kurze **Einführung** beim
  ersten Start.
- **Deutsch oder Englisch:** Die Sprache lässt sich in den Einstellungen umstellen. Standard ist die
  Sprache des Handys: Auf deutschen Geräten ist KackZug deutsch, auf allen anderen englisch.

Gespeichert wird das Muster einer Verbindung, also Bahnhöfe, Uhrzeiten und Zug, nicht das Datum. Die
Fahrt gilt dann an jedem Tag, an dem du die Schicht eingetragen hast.

## Installation

KackZug gibt es nicht im Play Store. Die fertige APK liegt auf der
[Release-Seite](https://github.com/BastianBaumann/KackZug/releases).

1. Auf dem Handy die neueste `KackZug-x.y.z.apk` herunterladen.
2. Die Datei öffnen. Android fragt beim ersten Mal, ob der Browser Apps installieren darf. Das einmal
   erlauben.
3. KackZug öffnen und der Einführung folgen.

### Updates

KackZug aktualisiert sich selbst. Die App schaut einmal am Tag auf GitHub nach, ob es eine neue Version
gibt, und zeigt dann oben einen Hinweis. Ein Tipp auf „Jetzt aktualisieren“ lädt die neue APK in der
App herunter, prüft sie und installiert sie als Update über die alte Version. Deine Schichten und der
Wochenplan bleiben erhalten.

Beim ersten Update fragt Android einmal, ob KackZug Apps installieren darf. Das einmal erlauben, danach
geht es von selbst weiter. Ab Android 12 laufen spätere Updates meist ohne weitere Rückfrage durch;
sonst zeigt Android vor dem Installieren noch einmal kurz seinen Bestätigungsdialog.

Die tägliche Prüfung lässt sich in den Einstellungen abschalten. Mit „Nach Updates suchen“ schaut die
App sofort nach. Vorabversionen (Tags mit Bindestrich, z. B. `v0.3.0-dev.1`) bietet sie nur an, wenn
„Vorabversionen anbieten“ eingeschaltet ist. Das sind Testversionen aus `dev`, die Fehler haben können.

Wer Updates lieber über [Obtainium](https://github.com/ImranR98/Obtainium) bezieht, trägt dort
`https://github.com/BastianBaumann/KackZug` ein.

## Damit die Alarme sicher ankommen

Android ist streng mit Apps, die im Hintergrund arbeiten. KackZug braucht deshalb:

- **Benachrichtigungen:** Die App fragt beim ersten Start danach.
- **Pünktliche Alarme:** Ist bei einer normal installierten APK schon erlaubt.
- **Akku-Optimierung aus:** In den Einstellungen von KackZug steht, ob das noch fehlt. Ein Tipp auf
  „Erlauben“ öffnet die passende Systemseite.
- **Bitte nicht stören (freiwillig):** Nur wenn du „Auch bei Bitte nicht stören“ einschaltest, fragt
  Android einmal nach dem Zugriff auf „Bitte nicht stören“.

Manche Hersteller (zum Beispiel Xiaomi, Huawei, Samsung oder OnePlus) haben zusätzlich eigene
Stromsparfunktionen, die Apps trotzdem schlafen legen. Welche Einstellung bei deinem Handy hilft,
steht auf [dontkillmyapp.com](https://dontkillmyapp.com). Mit dem Test-Alarm in den Einstellungen
kannst du prüfen, ob eine Meldung auch bei gesperrtem Bildschirm laut genug ankommt.

## Datenschutz

- Es gibt keine Konten, keine Werbung und keine Auswertung, was du in der App machst.
- Schichten, Wochenplan und Einstellungen liegen nur auf deinem Handy.
- Für die Prüfung fragt das Handy direkt bei bahn.de nach. Dabei sieht bahn.de die IP-Adresse des
  Handys, aber nicht, wer du bist oder welche Schichten du hast.
- Für den Update-Hinweis fragt das Handy einmal am Tag bei GitHub nach. Das lässt sich abschalten.
  Die neue Version lädt die App erst, wenn du auf „Jetzt aktualisieren“ tippst.

## Grenzen

- KackZug nutzt die Schnittstelle, die auch die Website bahn.de verwendet. Sie ist nicht offiziell
  dokumentiert und kann sich jederzeit ändern. Wenn bahn.de nicht antwortet, versucht die App es kurz
  danach noch einmal und meldet bei der letzten Prüfung vor der Abfahrt, dass sie nicht prüfen konnte.
- Schienenersatzverkehr taucht nicht als Zug auf. Die App meldet dann, dass die Fahrt so nicht fährt.
  Im DB Navigator steht, wie es stattdessen weitergeht.
- Nach dem Fahrplanwechsel im Dezember ändern sich oft Zeiten oder Zugnummern. Meldet KackZug dann
  „fährt so nicht mehr“, die Verbindung einfach neu teilen.
- Echtzeitdaten gibt es vor allem für Züge. Bei Straßenbahnen und Bussen erkennt die App meist nur
  Ausfälle, keine Verspätungen.
- Bei einer von Hand eingegebenen Verbindung kennt KackZug kein geplantes Gleis und meldet deshalb
  keinen Gleiswechsel.

## Hinweis

KackZug ist ein privates Open-Source-Projekt und hat nichts mit der Deutschen Bahn zu tun. „Deutsche
Bahn“, „DB Navigator“ und „bahn.de“ sind Namen und Marken ihrer Inhaber und werden hier nur verwendet,
um zu beschreiben, womit die App zusammenarbeitet.

## Selbst bauen

Voraussetzungen: .NET 10 SDK mit Android-Workload (`dotnet workload install android`), Android SDK
mit Plattform 36 und ein JDK 17.

```
dotnet build KackZug.slnx
dotnet test KackZug.slnx
```

Für die Entwicklung der Oberfläche gibt es einen Desktop-Start:
`dotnet run --project src/KackZug.Desktop`. Er zeigt dieselben Seiten in einem schmalen Fenster,
Alarme und Benachrichtigungen landen dort nur im Log.

Der Störungston wird mit `python tools/sound/build_alarm.py` erzeugt, die App-Grafiken mit
`python tools/design/build_assets.py`.

### Release erstellen

Ein Tag der Form `v1.2.3` startet den Release-Workflow. Er baut die APK, signiert sie mit dem
Release-Schlüssel aus der geschützten Umgebung `release` und veröffentlicht sie als GitHub-Release,
zusammen mit der Prüfsumme `KackZug-x.y.z.apk.sha256`, die das Update in der App kontrolliert. Damit
Updates über eine bestehende Installation gehen, muss es immer derselbe Schlüssel sein. Stabile
Versionen baut er nur von Commits, die auf `main` liegen; Tags darf nur der Maintainer setzen (siehe
`.github/rulesets/`).

## Mitmachen

Fehlerberichte, Ideen und Pull Requests sind willkommen. Wie das abläuft, steht in
[CONTRIBUTING.md](CONTRIBUTING.md). Pull Requests gehen gegen den Branch `dev`. Sicherheitslücken bitte
wie in [SECURITY.md](SECURITY.md) beschrieben privat melden. Es gelten die
[Verhaltensregeln](CODE_OF_CONDUCT.md).

## Lizenz

KackZug steht unter der GNU General Public License, Version 3 oder neuer (siehe [LICENSE](LICENSE)).
Das gilt auch für Logo und Symbole, die aus dem eigenen Designsystem des Projekts stammen
(`assets/`). Die Schrift ist [Inter](https://rsms.me/inter/) unter der SIL Open Font License.
