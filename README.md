<p align="center">
  <img src="assets/logo/kackzug-app-icon.svg" alt="KackZug Logo" width="160">
</p>

# KackZug

[Deutsch](README.de.md) · **English**

KackZug is a small Android app for anyone who commutes to shift work by train in Germany. You save
your shifts with the trip there and back, set which shift you work on which day, and the phone checks
on its own before departure whether your connection is still running as planned. If a train is
cancelled, delayed, changes platform or a connection gets tight, KackZug lets you know, loud and clear.

## What the app does

- **Import connections by sharing:** Look up a connection in DB Navigator, tap "Share" and pick
  KackZug. The connection, with all its legs (including walks and trams), is added to a shift as the
  outbound or return trip. If sharing does not work, you can also enter the connection by hand:
  search for the stations, then enter the times and the line.
- **Plan your week:** Each day gets a shift or stays free. The outbound and return trip can be turned
  off separately for each day. You can plan next week in advance, and with "Set several days"
  ("Mehrere Tage setzen", or a long press on a day) several days get the same shift at once. If you
  like, a new week takes over the plan of the previous week.
- **Details for every trip:** a large departure time, the current state of each leg (delay, new
  platform, cancellation) and how much time you have left to change trains. The "Open in DB
  Navigator" button ("Im DB Navigator öffnen") opens the connection for exactly that day in DB
  Navigator. If DB Navigator is not installed, it opens the connection on bahn.de in the browser
  instead and tells you so.
- **Checks at fixed times:** the evening before and shortly before departure, by default 90, 45 and
  15 minutes before. The times can be changed.
- **Loud alerts on disruptions:** cancellation, partial cancellation, a delay above a threshold you
  set, a platform change, a connection at risk, or a trip that no longer exists in this form. A
  full-screen alarm opens, even on the lock screen. It rings and comes back every five minutes until
  you tap "Seen". KackZug has its own shrill tone for this. If you want, it also rings in "Do not
  disturb" mode.
- **Colours you understand at a glance:** messages that everything is on time are green and come
  without a sound, problems are red and loud.
- **Honest status:** bahn.de has the timetable days in advance, but real-time data only shortly
  before departure. As long as there is no real-time data, KackZug shows "As scheduled" ("Laut
  Fahrplan") instead of "On time" ("Planmäßig"). If bahn.de has no usable data for a trip, it shows a
  grey "No data" instead of an alarm.
- **"Check now"** for today and tomorrow, a **test alarm** and a short **introduction** on first
  launch.
- **German or English:** The language can be switched in the settings. The default is the phone's
  language: on German devices KackZug is in German, on all others in English.

What is saved is the pattern of a connection, meaning stations, times and train, not the date. The
trip then applies on every day you have entered that shift.

## Installation

KackZug is not on the Play Store. The ready-made APK is on the
[releases page](https://github.com/BastianBaumann/KackZug/releases).

1. Download the latest `KackZug-x.y.z.apk` on your phone.
2. Open the file. The first time, Android asks whether the browser may install apps. Allow this once.
3. Open KackZug and follow the introduction.

### Updates

KackZug updates itself. Once a day the app checks GitHub for a new version and then shows a notice
at the top. Tapping "Update now" downloads the new APK inside the app, verifies it and installs it as
an update over the old version. Your shifts and weekly plan are kept.

On the first update Android asks once whether KackZug may install apps. Allow it once, after that it
carries on by itself. From Android 12 on, later updates usually go through without asking again;
otherwise Android briefly shows its confirmation dialog before installing.

The daily check can be turned off in the settings. "Check for updates" makes the app look right away.
Pre-releases (tags with a hyphen, e.g. `v0.3.0-dev.1`) are only offered when "Offer pre-releases" is
turned on. These are test builds from `dev` that may have bugs.

If you prefer to get updates through [Obtainium](https://github.com/ImranR98/Obtainium), add
`https://github.com/BastianBaumann/KackZug` there.

## Making sure the alarms get through

Android is strict with apps that work in the background. KackZug therefore needs:

- **Notifications:** The app asks for this on first launch.
- **Exact alarms:** Already allowed for a normally installed APK.
- **Battery optimisation off:** The KackZug settings show whether this is still missing. Tapping
  "Allow" opens the matching system page.
- **Do not disturb (optional):** Only if you turn on "Also in Do not disturb" does Android ask once
  for access to "Do not disturb".

Some manufacturers (for example Xiaomi, Huawei, Samsung or OnePlus) also have their own power-saving
features that put apps to sleep anyway. Which setting helps on your phone is explained on
[dontkillmyapp.com](https://dontkillmyapp.com). With the test alarm in the settings you can check
whether an alert arrives loud enough even when the screen is locked.

## Privacy

- There are no accounts, no ads and no tracking of what you do in the app.
- Shifts, weekly plan and settings are stored only on your phone.
- For the checks, the phone asks bahn.de directly. bahn.de sees the phone's IP address, but not who
  you are or which shifts you have.
- For the update notice, the phone asks GitHub once a day. This can be turned off. The app only
  downloads the new version when you tap "Update now".

## Limitations

- KackZug uses the interface that the bahn.de website itself uses. It is not officially documented
  and can change at any time. If bahn.de does not respond, the app tries again shortly afterwards
  and, at the last check before departure, tells you that it could not check.
- Rail replacement buses do not show up as a train. The app then reports that the trip is not
  running as planned. DB Navigator shows how to continue instead.
- After the timetable change in December, times or train numbers often change. If KackZug then
  reports "no longer runs like this", simply share the connection again.
- Real-time data is mainly available for trains. For trams and buses the app can usually only detect
  cancellations, not delays.
- For a connection entered by hand, KackZug does not know the planned platform and therefore does not
  report platform changes.

## Disclaimer

KackZug is a private open-source project and has nothing to do with Deutsche Bahn. "Deutsche Bahn",
"DB Navigator" and "bahn.de" are names and trademarks of their owners and are used here only to
describe what the app works with.

## Building it yourself

Requirements: .NET 10 SDK with the Android workload (`dotnet workload install android`), Android SDK
with platform 36 and a JDK 17.

```
dotnet build KackZug.slnx
dotnet test KackZug.slnx
```

For working on the user interface there is a desktop launcher:
`dotnet run --project src/KackZug.Desktop`. It shows the same pages in a narrow window; alarms and
notifications only end up in the log there.

The disruption tone is generated with `python tools/sound/build_alarm.py`, the app graphics with
`python tools/design/build_assets.py`.

### Creating a release

A tag of the form `v1.2.3` starts the release workflow. It builds the APK, signs it with the release
key from the protected environment `release` and publishes it as a GitHub release, together with the
checksum `KackZug-x.y.z.apk.sha256` that the in-app update verifies. For updates to install over an
existing installation, it always has to be the same key. Stable versions are only built from commits
that are on `main`; only the maintainer may set tags (see `.github/rulesets/`).

## Contributing

Bug reports, ideas and pull requests are welcome. How it works is described in
[CONTRIBUTING.md](CONTRIBUTING.md). Pull requests go against the `dev` branch. Please report security
vulnerabilities privately as described in [SECURITY.md](SECURITY.md). The
[Code of Conduct](CODE_OF_CONDUCT.md) applies.

## License

KackZug is licensed under the GNU General Public License, version 3 or later (see [LICENSE](LICENSE)).
This also applies to the logo and icons, which come from the project's own design system
(`assets/`). The font is [Inter](https://rsms.me/inter/) under the SIL Open Font License.
