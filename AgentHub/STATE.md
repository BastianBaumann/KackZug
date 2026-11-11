# Project State

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Protocol:** AgentStarter v4.4.0 @ abd4861
- **Current phase:** P5 EXECUTION
- **Scale:** S (D-008)
- **Mode / Autonomy:** A (solo) · confirm
- **Last updated:** 2026-10-22 by (@Core)
- **In-flight:** none; main = dev = origin = ba9c18c, tag v0.5.1 pushed (release run to check)
- **Next action:** developer checks the v0.5.1 release run; then new feedback; open: Taskboard compaction, design system DayRow update
- **Pending gates:** none (combined gate APPROVED 2026-10-20)
- **Pending human checks:** Actions fork approval + code security settings (not visible without login), keystore backup decision; device test checklist (T-022, T-023 look, T-029 swipe/battery, B1/B2/B4, T-030 fixes, T-035 keyboard, T-036 DND, T-037 manual entry live, T-038 Navigator app + number fields, T-039 green badge, T-040 alarm tone, T-042 Navigator opens the connection)
- **ID counters:** next E-14 · T-071 · D-014 · Q-011 · L-003
- **Milestones:** M1 MVP: open (T-022) · M2 / E-06 Feedback: open (T-023 look, T-029) · M3 / E-07 Planung+: released v0.3.0 · M4 / E-08 Oeffentliches Repo: released v0.3.1 · M5 / E-09 Feedback+Updater: released v0.4.0 · M6 / E-10 Feedback v0.4.0: released v0.4.1 · M7 / E-11: released v0.4.2 · M8 / E-12 Einzelfahrten: released v0.5.0 · M9 / E-13: released v0.5.1
- **Stack profiles:** dotnet, avalonia, dotnet-desktop-mobile · **Integrations:** bahn.de web API (unofficial, D-002)
- **CI entry point:** `dotnet test KackZug.slnx` + GitHub Actions ci.yml; release.yml on tag v*

## Project conventions (changes need an ADR)
- AI data in repo (K1): committed (2026-11-11, für den zentralen AgentHub-Server; vorher invisible)
- Branch flow (developer 2026-10-21): develop on feature/fix branches, merge into dev; pre-releases (tags with -) only from dev, stable tags only from main (release.yml checks both)
- Git rights (K2): commit+push · push target: current working branch only, never force, main only with explicit OK · 2026-10-21: main = dev @ b75de4e, released as v0.3.0 (developer OK); new work again on dev first
- Git identity (K3): Bastian Baumann, bastian.baumann99310@gmail.com, given by developer 2026-10-20, set repo-local on developer's instruction · authorship guard hook: not offered yet
- Authorship: the developer is always author/committer; no AI attribution anywhere (fixed rule)
- Doc language: app UI English + German (setting, default system), README.md English + README.de.md German; code, identifiers, commits English (D-011, supersedes D-006 language part)
- Dates: ISO 8601 dates · Code style: developer-preferences.md (D-006)
- Commit messages: Conventional Commits
- Developer data: agents never touch real data
- Third-party assets: logo.jpg and PLAN.md in project root are NOT ours / AI notes -> never commit (excluded locally)

## Index (read on demand; all AI data lives in AgentHub/)
- ./AGENTS.md: entry file for agents · ./.agentstarter/: protocol clone (never committed)
- Root exceptions in use: none
- Design source: Design System artifact "ZugKacke" https://claude.ai/artifact/7Uq5gAxSNUieFgi412nJaK (D-009)
- ./Vision.md: developer's original PLAN.md (read-only, partly superseded)
- ./SessionLog.md: last 5 handoffs · ./Taskboard.md: open work + current milestone · ./OpenQuestions.md: open questions
- ./ProjectBrief.md: requirements · ./DesignDocument.md: architecture & contracts · ./DecisionLog.md: ADR index → ./decisions/
- ./maintenance/: Setup · Operations · KnownIssues · Dependencies · AgentNotes
- ./boards/: _Common + role boards · ./Lessons.md · ./archive/
