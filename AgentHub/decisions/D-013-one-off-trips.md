# D-013: One-off trips next to shifts

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-22 · **Status:** accepted (by @Core; feature requested by @Dev/Bastian, details are clear winners)
- **Decision:** One-off trips are stored in their own list (AppData.Trips) with a fixed date, are checked through the same pipeline as shift trips (TripCalendar yields them on their date), get their own bottom tab "Fahrten", and are deleted automatically 60 minutes after the planned arrival of the last ride. Saving offers "Einzelfahrt" as an extra target in the existing import/manual-entry step, with a date (prefilled from the shared connection) and an optional name.
- **Why:** reuse of the proven check/alarm path; no new screens for saving; "gelöscht wenn vorbei" without user action; the 60 min buffer covers delays.
- **Consequences:** alert texts need a variant without direction; data schema gains a list (backwards compatible); v0.5.0.
