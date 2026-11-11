# D-002: Data source: unofficial bahn.de API by default, own-key Timetables API later

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** App is public open source; an embedded DB Marketplace key would be extractable from the APK. Share-link (vbid) resolution only works via bahn.de anyway.
- **Decision:** Import and live checks use the unofficial bahn.de/Vendo web API directly from the phone (no key, no own server), behind interfaces (connection resolver, status checker). Official Timetables API with a user-supplied key may be added later as an option.
- **Why:** No secrets in the APK, no friction for users, same data as the DB Navigator, few requests per phone.
- **Alternatives considered:** Timetables only with BYOK (high friction); Transitous (DB realtime coverage unclear); shipping a shared key (extractable, quota abuse).
- **Consequences:** API may change or block without notice -> fixtures, clear error messages, manual fallback; README states "unofficial, not affiliated with DB". Supersedes Vision.md sections 2 (API credentials) and 5 where they conflict.
