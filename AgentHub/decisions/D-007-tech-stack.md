# D-007: Tech stack and Android baseline

> MEMORY: read before acting; append, do not overwrite; this file (not the chat) is the source of truth; protocol: AgentStarter (see AgentHub/STATE.md -> Protocol).

- **Date:** 2026-10-20 · **Status:** accepted (2026-10-20, by @Dev/Bastian)
- **Decision:** .NET 10, Avalonia 12.x latest patch, CommunityToolkit.Mvvm, System.Text.Json, HttpClient, xUnit; no other packages. Projects Core/App/Android/Desktop + Core.Tests. Android minSdk 26, targetSdk 36, package id io.github.bastianbaumann.kackzug. Logo as SVG source + Android vector drawable + Avalonia geometry (no raster toolchain).
- **Why:** Matches Vision.md and the Avalonia stack profile (verified 12.0.4/12.1.3); minSdk 26 gives notification channels and adaptive icons; all deps GPL-compatible (MIT/Apache).
- **Alternatives considered:** PNG icons rendered via external tools (not installed, extra toolchain).
- **Consequences:** Android platform 36 must be installed (only 34/35 present).
