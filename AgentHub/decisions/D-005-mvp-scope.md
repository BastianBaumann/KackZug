# D-005: Slim MVP, fixed check times only

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** Vision.md is extensive; developer wants "einfach, simpel". Developer: 5-min follow-up checks are overkill for 2 trips/day and fight Android background limits.
- **Decision:** MVP = shifts with outbound/return via DB Navigator share, weekly plan (current week), checks at fixed times derived from departure (evening check day before + configurable lead times), loud problem notification, info notification, update hint. Not in MVP: follow-up checks every 5 min (dropped entirely), manual leg entry, full-screen alarm, next-week planning, own-key Timetables API.
- **Why:** Small, robust, few alarms; exact alarms per check are reliable under Doze.
- **Alternatives considered:** full Vision.md; even smaller (fixed weekdays per shift, no week plan).
- **Consequences:** Vision.md sections 6.1 (Folge-Checks), 4.3, 7 (Vollbild) deferred/dropped.
