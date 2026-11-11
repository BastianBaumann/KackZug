# Agent notes (environment gotchas)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- [2026-10-20] bahn.de blocks custom User-Agents (403) and answered OPS_BLOCKED after ~6 requests in 2 min. Do live calls sparingly, never in loops/tests. Responses can be gzip even without Accept-Encoding (use curl --compressed).
- [2026-10-20] adb/emulator are not on PATH: use %LOCALAPPDATA%/Android/Sdk/... Only JDK 8 on PATH; the Android build finds JDK 17 itself.
- [2026-10-20] Bash heredocs: always quote the delimiter ('EOF'), otherwise backticks run as commands.
- [2026-10-21] Bash tool: a heredoc containing "!" (e.g. C# `previous!.`) fails to parse ("unexpected EOF"). Write patch scripts with the Write tool into the scratchpad and run them with python.
- [2026-10-22] bahn.de can split one train into two journeys (e.g. RE3 3962 to Weimar, continuing as 18682 with ZE#3962 to Erfurt); the fahrt stop list then ends early. Look for the same ZE number on the last stop's board. 'irregulaereVerkehrstage' in fahrt marks planned timetable changes.
