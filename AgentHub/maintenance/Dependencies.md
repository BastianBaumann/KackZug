# Dependencies (packages, tools, images: version · license · why)

> 🧠 **MEMORY: read before acting · append, don't overwrite · this file (not the chat) is the source of truth · protocol: AgentStarter (see AgentHub/STATE.md → Protocol).**

| Name | Version | License | Purpose | Notes (pins, advisories) |
|---|---|---|---|---|
| .NET SDK | 10.0.300 | MIT | runtime/build | android workload 36.1.69 |
| Avalonia, Avalonia.Themes.Fluent, Avalonia.Desktop, Avalonia.Android | 12.1.4 | MIT | UI | all Avalonia packages same version |
| CommunityToolkit.Mvvm | 8.4.2 | MIT | MVVM | |
| xunit | 2.9.3 | Apache-2.0 | tests | |
| xunit.runner.visualstudio | 3.1.5 | Apache-2.0 | tests | |
| Microsoft.NET.Test.Sdk | 18.11.0 | MIT | tests | |
| Android SDK platform | 36 (+ build-tools 36.0.0) | Android SDK license | build | %LOCALAPPDATA%/Android/Sdk, picked by csproj condition |
| JDK | 17 (C:/Program Files (x86)/Android/openjdk) | GPLv2+CE | Android build | auto-detected |
| Avalonia.Controls.ColorPicker | 12.1.4 | MIT | colour pickers in settings | needs StyleInclude in App.axaml |
| Avalonia.Fonts.Inter | 12.1.4 | MIT (font: SIL OFL 1.1) | Inter font from the design system | `.WithAppFont()` in both heads |
| Design system assets (logo, icons) | ZugKacke 2026-10-20 | developer's own, GPL-3.0-or-later in repo | assets/logo, assets/icons -> tools/design/build_assets.py | MDI icons removed |
