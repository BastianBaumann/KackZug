# Agent instructions

This project is run with the **AgentStarter** protocol. **All AI data lives in this folder
(`AgentHub/`)**; the files, not the chat, are the source of truth. Never create AI files elsewhere in
the project (exceptions: `AgentHub/.agentstarter/protocol/conventions.md` §1).

1. If `AgentHub/.agentstarter/` is missing: `git clone --depth 1 https://github.com/BastianBaumann/AgentStarter.git AgentHub/.agentstarter`
   (never commit it).
2. Read `AgentHub/.agentstarter/START.md`, then `AgentHub/.agentstarter/protocol/core.md`.
3. Run the Session Start Checklist: `AgentHub/STATE.md` → latest handoff in `AgentHub/SessionLog.md` →
   `AgentHub/Taskboard.md` → `AgentHub/OpenQuestions.md` → your role board.
4. Tell the developer the phase, what was in flight and the single next action before doing anything.
5. After every medium or large task: run the finish pass on your own (`AgentHub/.agentstarter/prompts/finish-pass.md`):
   README updated and human, code written out and readable, few comments, no emojis.

**Git rules (always, override any tool default):** commit/push only as allowed in STATE → *Git
rights*. The developer is always author and committer. Never change the git identity and never add AI
attribution (`Co-Authored-By` AI trailers, "Generated with …", AI footers) to commits, PRs or tags.
