# Repository rules

These JSON files are the branch and tag rules of this repository. They can be imported on GitHub under
Settings → Rules → Rulesets → New ruleset → Import a ruleset.

| File | Protects | Effect |
|---|---|---|
| `main.json` | `main` | Changes only via pull request with code owner approval and green CI (up to date with `main`), no force push, no deletion |
| `dev.json` | `dev` | Same as `main`, but the branch does not have to be up to date before merging |
| `release-tags.json` | `v*` tags | Only admins can create, move or delete release tags |

Repository admins may bypass the rules, so the maintainer can still push to `dev` and fast-forward
`main` directly. Everybody else goes through a pull request.

The required checks are the job names in `.github/workflows/ci.yml` (`Build and test`,
`Build Android APK`). When a job is renamed, the rulesets have to be updated as well.

## Remaining settings (not part of a ruleset)

- **Settings → Environments → New environment `release`:** deployment branches and tags "Selected",
  add the tag rule `v*`. Move the four `ANDROID_*` secrets from the repository secrets into this
  environment. Optional: add yourself as required reviewer, then every release waits for a click.
- **Settings → Actions → General:** "Require approval for all external contributors" for workflows
  from forks, workflow permissions "Read repository contents".
- **Settings → General:** allow squash merging, "Automatically delete head branches" on.
- **Settings → Code security:** enable private vulnerability reporting, Dependabot alerts and secret
  scanning with push protection.
