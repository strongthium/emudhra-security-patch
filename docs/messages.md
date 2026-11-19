<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › Messages and exit codes

# Messages and exit codes

Everything `emudhra-patch` can print, by when it prints it, and the exit code it ends with. Errors go to standard
error and start with `error:`; everything else goes to standard output.

## Exit codes

| | |
|---|---|
| `0` | nothing on the list is trusted (any more); or nothing needed doing, or it was answered with no |
| `1` | an error |
| `2` | something on the list is still trusted |
| `3` | `revoke` or `restore` not run as administrator (Windows) or root (Linux) |

## Before it starts

| | Exit code |
|---|---|
| `error: this needs OpenSSL 3 (libssl.so.3), which .NET reads certificates with, and it isn't installed.` and what to install | `1` (Linux). See [On Linux](linux.md#openssl) |
| `unknown argument: <argument>` and the help | `1`. Something it doesn't know was given, or `--list` with no file after it. See [The command line](command-line.md) |
| `error: this runs on Windows and Linux.` | `1`: `scan`, `revoke` and `restore` on any other system |
| `error: revoke changes the machine's certificate stores: run it as administrator (or with --current-user for your own stores only).` | `3` (Windows). The same for `restore` |
| `error: revoke changes the system's CA certificates: run it as root (sudo).` | `3` (Linux). The same for `restore` |

## Reading the list

| | |
|---|---|
| `13 certificates in <file>` | how many certificates the list has, and which file it read |
| `note: <name>: no certs/<SHA-256>.pem, so it is only removed where it is found, not distrusted ahead of time.` | that line has no PEM. See [Changing the list](changing-the-list.md#certs) |
| `note: <name>: badcerts.txt says serial <serial>, the certificate has <serial>.` | the serial on the line isn't the PEM's. It goes on |
| `error: badcerts.txt was not found beside the program or in the current directory; say where it is with --list.` | exit code `1` |
| `error: <file>, line <n>: the last field is not a SHA-256 fingerprint (64 hex digits).` | exit code `1`. Fix that line, or put `#` in front of it |
| `error: certs/<SHA-256>.pem is not that certificate: its fingerprint is <SHA-256>.` | exit code `1`. The PEM and its name don't agree |

A list or a PEM that can't be read at all (a `--list` file that isn't there, a PEM that isn't one) stops it with
`error:` and the system's own message, and exit code `1`.

## scan

For each certificate, `[TRUSTED   ]`, `[distrusted]` or `[not here  ]`, its name, its fingerprint, and the places
it's in: see [scan](scan.md#what-it-prints). Then:

| | Exit code |
|---|---|
| `Nothing on the list is trusted on this computer.` | `0` |
| `<n> trusted. Run "emudhra-patch revoke" as administrator to distrust them; it also distrusts the ones with a PEM that are not here yet.` | `2`. `as root` on Linux |

## revoke

| | |
|---|---|
| `Distrust these <n> certificates on this computer? A backup goes to <folder>. [y/N]` | the question. `y` goes on |
| `Nothing was changed.` | it was answered with anything but `y`. Exit code `0` |
| `error: revoke asks before it changes anything; pass --yes when nobody is there to answer.` | nobody was there to answer, and there was no `--yes`. Nothing was changed. Exit code `1` |
| `Checking again:` | after the changes, before it looks through the stores again |
| `Done: nothing on the list is trusted any more. "emudhra-patch restore" undoes it.` | exit code `0` |
| `<n> still trusted (see above).` | exit code `2`. See [revoke](revoke.md#when-something-is-still-trusted) |

### revoke on Windows

| | |
|---|---|
| `removed from <store>: <name>` | taken out of a store that trusted it, for example `LocalMachine\AuthRoot` |
| `distrusted in LocalMachine\Disallowed: <name>` | put into **Disallowed**. `CurrentUser\Disallowed` with `--current-user` |

### revoke on Linux

| | |
|---|---|
| `removed from <file>` | a file of the machine's own, written again without the listed certificates |
| `deleted <file> (nothing else was in it)` | a file of the machine's own that held only listed certificates |
| `left in <file> (a p11-kit file; the blocklist overrules it)` | a `.p11-kit` file of the machine's own, not changed |
| `deselected in /etc/ca-certificates.conf: <line>` | Debian and Ubuntu: a root of the distribution's, deselected |
| `distrusted in <blocklist>: <name>` | a PEM put into a p11-kit blocklist |
| `WARNING: <file> is the distribution's and this system has no p11-kit blocklist to overrule it` | that certificate stays trusted |
| `<tool>: <line>` | what `update-ca-certificates` or `update-ca-trust` printed |
| `rebuilt the bundles: <tool>` | the tool finished |
| `WARNING: <tool> failed with exit code <n>` | the tool failed; its lines above say why |
| `WARNING: neither update-ca-certificates nor update-ca-trust is installed: the bundles were not rebuilt` | the bundles programs read are as they were |

See [On Linux](linux.md#what-revoke-changes).

## restore

| | |
|---|---|
| `Nothing to restore: there is no <folder>/manifest.txt.` | no `revoke` to undo. Exit code `0` |
| `no longer distrusted in <store>: <name>` | Windows: taken out of **Disallowed** again |
| `put back into <store>: <name>` | Windows: put back into the store it was taken out of |
| `no longer distrusted: deleted <file>` | Linux: a blocklist file it had made, deleted |
| `selected again in /etc/ca-certificates.conf: <line>` | Debian and Ubuntu: the `!` taken away again |
| `put back: <file>` | Linux: a file of the machine's own, as it was |
| `Restored.` | done. Exit code `0` |

On Linux, the lines of the tools that build the bundles follow, as for `revoke`. See
[The backup and restore](backup-and-restore.md).

## about and update

| | |
|---|---|
| `Asking GitHub for the latest version...` | |
| `You have the latest version.` | exit code `0` |
| `This is newer than the latest release, <version>.` | exit code `0` |
| `Version <version> is out. You have <version>. What's new: <address>` | |
| `Update to <version> now? It's checked against the release's checksums first. [y/N]` | `y` installs it; anything else, exit code `0` |
| `Updated to <version>. Certificates it distrusted stay distrusted; run "emudhra-patch scan" to see the new list against this computer.` | exit code `0` |
| `error: update asks before it changes anything; pass --yes when nobody is there to answer.` | exit code `1`. Nothing was changed |

When something goes wrong, it prints `error:` and one of these, then
`The releases are at https://github.com/strongthium/emudhra-security-patch/releases/latest`, and ends with exit code
`1`:

| | |
|---|---|
| `GitHub has had too many questions from this address for now. Try again in an hour.` | |
| `GitHub took too long to answer.` | no answer in 20 seconds |
| `GitHub's answer couldn't be read.` | |
| `GitHub's latest release is called "<tag>", which isn't a version.` | |
| `The release has no <archive>.` | the latest release has no archive for this system |
| `The release has no SHA256SUMS to check it against.` | nothing was installed |
| `SHA256SUMS doesn't list <archive>.` | nothing was installed |
| `The download of <archive> doesn't match the release's SHA256SUMS, so nothing was changed.` | a damaged download. Try again |
| `The download doesn't hold emudhra-patch.exe.` | `emudhra-patch` on Linux |
| `This program can't be replaced from here: <folder> needs an administrator to change it.` | run `update` as administrator, or with `sudo` |
| the system's own message | for example, when there's no connection to GitHub |

## Anything else

Anything else that goes wrong reading or changing a file or a certificate store stops it with `error:` and the
system's own message, and exit code `1`.

← [Changing the list](changing-the-list.md) · [Contents](README.md) · [Building](building.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
