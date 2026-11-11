# Lessons (project knowledge; generic ones marked upstream: proposed)

> 🧠 **MEMORY: read before acting · append, don't overwrite · this file (not the chat) is the source of truth · protocol: AgentStarter (see AgentHub/STATE.md → Protocol).**

| ID | Date | Kind | Scope | Lesson | Upstream |
|---|---|---|---|---|---|
| L-xxx | YYYY-MM-DD | FACT/LESSON/PATTERN/FAILURE | global/role/stack | <one or two sentences> | no / proposed / merged |
| L-001 | 2026-10-20 | FAILURE | stack (Avalonia) | Rebuilding row view models while clearing a SHARED ItemsSource made the old, still bound ComboBoxes write SelectedItem=null back, which silently set the in-memory plan to "frei" (file only safe by luck). Give each rebuild fresh collections, detach old row VMs, and ignore null selections. Caught by a screenshot-tour check, not by looking. | merged (AgentStarter v4.3.0) |
| L-002 | 2026-10-20 | FACT | stack (Android) | Stock Android keeps AlarmManager alarms when the app is swiped from recents (process is cold-started for the broadcast); vendor ROMs often treat the swipe as force stop and drop alarms unless the app is battery-unrestricted/autostart-allowed. Test "app killed" with a real swipe, not only `am kill`. | merged (AgentStarter v4.3.0) |
