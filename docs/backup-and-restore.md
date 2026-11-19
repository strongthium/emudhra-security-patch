<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › The backup and restore

# The backup and restore

```
emudhra-patch restore
```

Before `revoke` changes anything, it writes the change down and keeps a copy of what it's about to remove or edit.
`restore` reads that back and undoes it all, the last change first, so the computer's trust stores are as they were
before `revoke`.

## Where it is

| | |
|---|---|
| Windows | `%ProgramData%\Strongthium\emudhra-security-patch` (usually `C:\ProgramData\...`) |
| Windows, with `--current-user` | `%LocalAppData%\Strongthium\emudhra-security-patch`, the user's own |
| Linux | `/var/lib/emudhra-security-patch` |

`revoke` says where, in its question.

## What's in it

| | |
|---|---|
| `manifest.txt` | a line for each change `revoke` made, its fields separated by tabs, the kind of change first. `restore` reads this |
| `<time>-LocalMachine-Root-<SHA-256>.cer` (Windows) | a copy of a certificate taken out of a store: here, out of the machine's **Root** store. `<time>` is when, in UTC, to the thousandth of a second |
| `<time>-<file name>` (Linux) | a copy of a file of the machine's own CA certificates, as it was before `revoke` took a listed certificate out of it, or deleted it |
| `manifest.restored-<time>.txt` | a manifest `restore` has undone, kept |

The kinds of change in `manifest.txt`:

| | |
|---|---|
| `win-removed` | a certificate taken out of a store on Windows: the store, the fingerprint and the copy |
| `win-disallowed` | a certificate put into a **Disallowed** store on Windows |
| `file` | a file of the machine's own CA certificates changed or deleted on Linux, and its copy |
| `deb-deselected` | a root deselected in `/etc/ca-certificates.conf` on Debian or Ubuntu |
| `blocked` | a file put into a p11-kit blocklist on Linux |

## restore

`restore` needs the same rights as `revoke`: administrator on Windows, root on Linux. With `--current-user`, it
undoes a `revoke --current-user`, from the user's own backup; without it, the machine's.

It goes through `manifest.txt` from the end, and for each change:

| Change | What `restore` does | What it prints |
|---|---|---|
| `win-disallowed` | takes the certificate out of **Disallowed** again | `no longer distrusted in LocalMachine\Disallowed: <name>` |
| `win-removed` | puts the copy back into the store it came from, unless it's there already | `put back into LocalMachine\Root: <name>` |
| `blocked` | deletes the blocklist file | `no longer distrusted: deleted <file>` |
| `deb-deselected` | takes the `!` away from the root's line in `/etc/ca-certificates.conf` | `selected again in /etc/ca-certificates.conf: <line>` |
| `file` | copies the file back as it was, over what's there now | `put back: <file>` |

On Linux it then builds the bundles again, as `revoke` does. Then it renames `manifest.txt` to
`manifest.restored-<time>.txt`, so that the next `revoke` starts a manifest of its own, and prints `Restored.`

Only what `revoke` did is undone. A certificate that was in **Disallowed** or a blocklist before `revoke` ran wasn't
put there by it, isn't in the manifest, and stays.

When there's no `manifest.txt` (no `revoke` since the last `restore`), it says
`Nothing to restore: there is no <path>.` and changes nothing.

The copies stay in the folder after a `restore`.

## Without the list

`restore` works from `manifest.txt` and the copies alone: it doesn't read `badcerts.txt`, so a list changed since
`revoke` (by hand, or by [`update`](installing-and-updating.md#update)) makes no difference to what it undoes. `update`
doesn't touch the backup.

← [revoke](revoke.md) · [Contents](README.md) · [On Windows](windows.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
