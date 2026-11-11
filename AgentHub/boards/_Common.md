# Common Rules (all roles)

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Rules version:** v1 (2026-10-20)

## Quality bar
- Medium (D-006): xUnit for core logic with hand-made fixtures, functional checks for UI · CI entry point `dotnet test KackZug.slnx` (+ GitHub Actions ci.yml) must be green before [x]

## Conventions
- Code style: AgentHub/.agentstarter/knowledge/developer-preferences.md · commits: Conventional Commits in English, no task IDs · doc language: UI English + German, README.md English + README.de.md (D-011)

## Always
- Work only on claimed tasks inside your ownership; cross-role changes via NOTE/QUESTION.
- Record VERIFY with command → counts; list what was not verified.
- Doc sync after behavior/setup changes; no secrets or sensitive identifiers in any file.
- Finish pass after every medium or large task: README in a human tone, code written out, few
  comments, no emojis (`AgentHub/.agentstarter/prompts/finish-pass.md`).
- Git: only within the K2 rights in STATE; the developer is always author/committer; no AI attribution;
  with K1 = ignored/invisible no task IDs or AgentHub references in commits, code comments or docs.
- Open source: only own assets; no DB logos, screenshots or "ICE"/DB marks in assets; GPL-3.0-compatible dependencies only.

## Rules changelog
- [2026-10-20] v1 created.
