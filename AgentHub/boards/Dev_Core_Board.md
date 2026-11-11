# Dev Board: Core

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Mission:** builds and verifies all of KackZug (core logic, UI, Android head, CI, docs).
- **Rules version:** v1 (2026-10-20) · shared rules: ./_Common.md
- **Role definition:** AgentHub/.agentstarter/roles/core.md

## Ownership
- Owns: everything in the repo (see Taskboard ownership map) · Does NOT own: keystore, GitHub secrets, real-device tests (@Dev/Bastian)

## Role-specific rules & boundaries
- Stack profiles: AgentHub/.agentstarter/stacks/avalonia/ (conventions, pitfalls, recipes/android.md), dotnet.md, dotnet-desktop-mobile.md.
- bahn.de calls only via Core interfaces; keep request volume minimal; browser-like User-Agent required (own UA gets 403, design §6.1).
- Live API calls during development: only a handful, manual, never in unit tests.
- Only self-made assets; never commit logo.jpg or PLAN.md.

## Definition of Done (role additions)
- Android changes checked on emulator + `adb logcat -d`.

## Log (append; one line per task start/finish)

## Rules changelog
- [2026-10-20] v1 created.
