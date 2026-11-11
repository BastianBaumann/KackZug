# D-001: Working agreement (kickoff K1-K3)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Context:** New project KackZug, GitHub repo BastianBaumann/KackZug (private now, public later).
- **Decision:** K1 = invisible (AgentHub/ only via .git/info/exclude). K2 = commit + push (agent pushes to working branch; developer is author). K3 = Bastian Baumann <bastian.baumann99310@gmail.com>, set repo-local because the developer named it explicitly.
- **Why:** Repo will be public open source; no AI traces wanted; developer must be the author.
- **Alternatives considered:** committed (AI data public) -> no.
- **Consequences:** No task/ADR IDs in commits, code, docs. AgentHub must be backed up separately by the developer. logo.jpg (third-party) and PLAN.md excluded locally.
