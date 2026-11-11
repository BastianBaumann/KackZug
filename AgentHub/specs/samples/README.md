# Live samples from bahn.de (PRIVATE, never commit; contains the developer's real commute)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

Captured 2026-10-20 ~17:16 from two links the developer shared ("Kopieren" in DB Navigator share sheet).
- verbindung-1.json / recon-1.json: RE1 3664 Jena West 17:38 -> Erfurt Hbf 18:07 (+8 min live), walk, STR 3 18:16 -> Bergstrasse 18:25 (transfer missed)
- verbindung-2.json / recon-2.json: RB21 80664 18:07 -> 18:43 (on time), walk, STR 6 18:53 -> 19:02
- Shared text was English ("View journey: https://int.bahn.de/en/buchung/start?vbid=...") -> parse only the URL.
- Committed test fixtures must use invented stations/times, derived from these shapes only.
- [2026-10-20 2nd session] board-1.json: departures Jena West 2026-10-21 17:30 (includes nearby bus stops; RE1 3664 at 17:38, journeyId has CA#NRE and ZE#3664; board key spelled "verkehrmittel", vm.linienNummer "RE1"); trip-1.json: /reiseloesung/fahrt for that journeyId (top-level cancelled, halte[] with extId, abfahrt/ankunft sollzeit, gleis, kategorie, nummer).
