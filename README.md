<p align="center">
  <img src="assets/logo/kackzug.svg" alt="KackZug Logo" width="180">
</p>

# KackZug

KackZug ist eine kleine Android-App für alle, die im Schichtdienst mit der Bahn zur Arbeit fahren. Du
speicherst deine Schichten mit Hin- und Rückfahrt, legst fest, an welchem Tag du welche Schicht hast,
und das Handy prüft vor der Abfahrt selbst, ob deine Verbindung noch so fährt. Fällt ein Zug aus, ist
er verspätet, wechselt das Gleis oder wird der Anschluss knapp, meldet sich KackZug laut und deutlich.

## Was die App kann

- **Verbindungen per Teilen übernehmen:** Im DB Navigator eine Verbindung suchen, auf „Teilen“ tippen
  und KackZug auswählen. Die Verbindung landet mit allen Teilstrecken (auch Fußwege und Straßenbahn)
  als Hin- oder Rückfahrt in einer Schicht.
- **Woche planen:** Jeder Tag bekommt eine Schicht oder bleibt frei. Hin- und Rückfahrt lassen sich
  pro Tag einzeln abschalten. Eine neue Woche übernimmt auf Wunsch den Plan der Vorwoche.
- **Prüfen zu festen Zeiten:** am Abend vorher und kurz vor der Abfahrt, standardmäßig 90, 45 und
  15 Minuten vorher. Die Zeiten sind einstellbar.
- **Laute Meldungen bei Störungen:** Ausfall, Teilausfall, Verspätung ab einer einstellbaren Schwelle,
  Gleiswechsel, gefährdeter Anschluss oder eine Fahrt, die es so nicht mehr gibt. Der Alarmton kann
  sich wiederholen, bis du reagierst.
- **„Jetzt prüfen“** für heute und morgen, ein **Test-Alarm** und eine kurze **Einführung** beim
  ersten Start.

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

KackZug schaut einmal am Tag auf GitHub nach, ob es eine neue Version gibt, und zeigt dann oben einen
Hinweis mit Link zur Release-Seite. Die neue APK einfach herunterladen und öffnen: Android installiert
sie als Update über die alte Version, deine Schichten und der Wochenplan bleiben erhalten. Die Prüfung
lässt sich in den Einstellungen abschalten.

Wer Updates automatisch haben möchte, kann [Obtainium](https://github.com/ImranR98/Obtainium)
verwenden und dort `https://github.com/BastianBaumann/KackZug` eintragen.

## Damit die Alarme sicher ankommen

Android ist streng mit Apps, die im Hintergrund arbeiten. KackZug braucht deshalb:

- **Benachrichtigungen:** Die App fragt beim ersten Start danach.
- **Pünktliche Alarme:** Ist bei einer normal installierten APK schon erlaubt.
- **Akku-Optimierung aus:** In den Einstellungen von KackZug steht, ob das noch fehlt. Ein Tipp auf
  „Erlauben“ öffnet die passende Systemseite.

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

### Release erstellen

Ein Tag der Form `v1.2.3` startet den Release-Workflow. Er baut die APK, signiert sie mit dem
Release-Schlüssel aus den Repository-Secrets und veröffentlicht sie als GitHub-Release. Damit Updates
über eine bestehende Installation gehen, muss es immer derselbe Schlüssel sein.

## Lizenz

KackZug steht unter der GNU General Public License, Version 3 oder neuer (siehe [LICENSE](LICENSE)).
Das gilt auch für das Logo. Die Symbole in der App stammen aus den
[Material Design Icons](https://pictogrammers.com/library/mdi/) und stehen unter der Apache License 2.0.
