# Mitmachen bei KackZug

Schön, dass du helfen willst. Fehlerberichte, Ideen und Pull Requests sind willkommen.

## Fehler melden und Ideen einbringen

- **Fehler:** ein [Issue](https://github.com/BastianBaumann/KackZug/issues/new/choose) mit der Vorlage
  „Fehler melden“ anlegen. Wichtig sind App-Version (Einstellungen), Handy und Android-Version und die
  Schritte, mit denen der Fehler auftritt.
- **Ideen:** die Vorlage „Idee vorschlagen“ nutzen. Bei größeren Änderungen bitte erst ein Issue
  aufmachen und kurz abstimmen, bevor du viel Zeit in Code steckst.
- **Sicherheitslücken** bitte nicht öffentlich melden, sondern wie in [SECURITY.md](SECURITY.md)
  beschrieben.

## Branches

| Branch | Zweck |
|---|---|
| `main` | Stand der letzten stabilen Version. Nur der Maintainer führt `dev` hier zusammen. |
| `dev` | Entwicklung. **Pull Requests gehen immer nach `dev`.** |

Releases entstehen nur aus Tags (`v1.2.3` von `main`, Vorabversionen wie `v1.3.0-dev.1` von `dev`).
Tags kann nur der Maintainer setzen, und nur er hat Zugriff auf den Signierschlüssel.

## Ablauf für einen Pull Request

1. Repository forken und einen Branch von `dev` anlegen, zum Beispiel `fix/teilen-absturz`.
2. Änderung machen, Tests ergänzen, lokal bauen und testen (siehe unten).
3. Pull Request gegen `dev` öffnen und die Vorlage ausfüllen.
4. Die CI muss grün sein und der Maintainer muss zustimmen, erst dann wird gemergt.

## Bauen und testen

Voraussetzungen: .NET 10 SDK mit Android-Workload (`dotnet workload install android`), Android SDK
mit Plattform 36 und ein JDK 17.

```
dotnet test tests/KackZug.Core.Tests/KackZug.Core.Tests.csproj
dotnet build src/KackZug.Desktop/KackZug.Desktop.csproj
dotnet build src/KackZug.Android/KackZug.Android.csproj -c Release
```

Für Änderungen an der Oberfläche gibt es den Desktop-Start (`dotnet run --project src/KackZug.Desktop`)
und eine Screenshot-Tour, die alle Seiten durchklickt und prüft (nur im Debug-Build):

```
KACKZUG_SCREENSHOTS=<ordner> KACKZUG_DATA_DIR=<leerer ordner> KACKZUG_DEMO=1 dotnet run --project src/KackZug.Desktop
```

Danach liegen Bilder und eine `checks.txt` im Ordner, jede Zeile sollte mit `OK` beginnen. Neue
Ansichten bekommen einen Schritt in `src/KackZug.Desktop/TourSteps.cs`.

## Regeln für Code

- Code, Bezeichner und Commit-Messages auf Englisch. Texte in der App gibt es auf Englisch und Deutsch:
  neue Texte kommen in beide Tabellen (`Strings.resx` und `Strings.de.resx` in `src/KackZug.Core/Localization`,
  `UiStrings.resx` und `UiStrings.de.resx` in `src/KackZug.App/Localization`). Das README gibt es auf Englisch
  (`README.md`) und Deutsch (`README.de.md`), beide werden zusammen angepasst.
- Ein Typ pro Datei, Logik ohne Oberfläche gehört nach `src/KackZug.Core` und bekommt Tests.
- Commit-Messages nach [Conventional Commits](https://www.conventionalcommits.org/de/), zum Beispiel
  `fix: keep one page view per main view`.
- Keine echten Daten, Zugangsdaten oder Schlüssel in Code, Tests oder Issues.
- Die App fragt bahn.de direkt. Bitte keine Änderungen, die dort viele Anfragen auslösen.

## Lizenz

Mit einem Pull Request stimmst du zu, dass dein Beitrag unter der
[GNU General Public License v3 oder neuer](LICENSE) veröffentlicht wird.
