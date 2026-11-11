# Spec: One-off trips (Einzelfahrten), v0.5.0

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Origin:** developer request 2026-10-22 ("einzelne Verbindungen einfügen ... einmalige Reise ... gelöscht wenn vorbei ... eigener Tab ... Option zum einmaligen Einspeichern, Datum wichtig")
- **Decision record:** D-013

## Behaviour
1. Saving a connection (share import or "Von Hand eingeben") offers, next to the shift targets, a target "Einzelfahrt" (one-off trip). With it: date field (prefilled with the day of the shared connection, manual entry: today), optional name (default "From → To"), no Hin/Rück choice. A date in the past is refused with a hint.
2. One-off trips are checked exactly like shift trips: same lead times, evening check, alerts, alarm screen, DND, silent green info. Alert titles use the trip name without "Hinfahrt/Rückfahrt".
3. New bottom tab "Fahrten" / "Trips" (train icon) between "Woche" and "Schichten": upcoming one-off trips sorted by departure. Card: name, weekday + date, first departure → last arrival, from → to, status badge + "geprüft HH:mm" (same badge logic as the week), buttons "Details" (trip detail with legs/live data) and "Im DB Navigator öffnen", delete (confirm dialog). Empty state explains how to add one.
4. A one-off trip is removed automatically once it is over: planned arrival of the last ride (day offsets respected) + 60 minutes has passed. Its check states go with it. Runs in DataMaintenance (app start, background checks, nightly maintenance).
5. The week view is unchanged.

## Data
- AppData.Trips: List<OneOffTrip> { Id (Guid), Name, TravelDate (DateOnly), Connection (SavedConnection), CreatedAt }.
- Check states / planned checks keyed like shifts (ShiftId = trip id, Direction = Outbound).
- JSON source-gen context updated; old files without "trips" load fine.

## Done when
- Core tests: calendar/planner include one-off trips on their date only; alert text without direction; removal after arrival + 60 min (incl. over midnight); import save validation.
- Tour: save a shared connection as one-off trip, tab shows card with date, details opens, delete works, expired trip disappears; English check of the tab.
- README both languages; Android build; emulator look.
