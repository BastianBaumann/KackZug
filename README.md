# KackZug

KackZug ist eine kleine Android-App für Pendler im Schichtdienst. Du speicherst deine Schichten mit
Hin- und Rückfahrt, legst fest, an welchen Tagen du welche Schicht hast, und das Handy prüft vor der
Abfahrt selbst, ob deine Züge noch fahren. Fällt etwas aus oder ist ein Zug verspätet, meldet sich die
App laut und deutlich.

Die App ist noch in Entwicklung.

## Wie es funktionieren soll

1. Im DB Navigator eine Verbindung suchen und über "Teilen" an KackZug schicken.
2. Die Verbindung als Hin- oder Rückfahrt einer Schicht speichern.
3. Im Wochenplan jedem Tag eine Schicht zuordnen.
4. KackZug prüft die Fahrten zu festen Zeiten vorher und meldet Probleme.

## Hinweis

KackZug ist ein privates Open-Source-Projekt und hat nichts mit der Deutschen Bahn zu tun. Die App
nutzt die öffentlich erreichbare, aber nicht offiziell dokumentierte Schnittstelle von bahn.de. Die
kann sich jederzeit ändern.

## Selbst bauen

Voraussetzungen: .NET 10 SDK mit Android-Workload (`dotnet workload install android`), Android SDK
mit Plattform 36 und ein JDK 17.

```
dotnet build KackZug.slnx
dotnet test KackZug.slnx
```

## Lizenz

GPL-3.0-or-later, siehe [LICENSE](LICENSE). Das gilt auch für das Logo.
