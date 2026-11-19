<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › revoke

# revoke

```
emudhra-patch revoke
emudhra-patch revoke --yes
```

Distrusts the certificates of the list on this computer: it takes each one out of every place that trusts it, and
distrusts every one whose certificate it has, whether the computer has it yet or not. A backup of everything it
changes is kept first, and [`restore`](backup-and-restore.md) undoes it.

It changes the whole computer's trust stores, so it's run as administrator on Windows (in a terminal opened with
**Run as administrator**) and as root on Linux (`sudo`). Without that, it stops before doing anything, with exit code
`3`. On Windows, `--current-user` changes only your own stores and needs no administrator: see
[On Windows](windows.md#only-your-own-stores).

## What it does

1. Reads the list and looks through the trust stores, and prints where each certificate is, as
   [`scan`](scan.md) does.
2. Asks:

   ```
   Distrust these 13 certificates on this computer? A backup goes to C:\ProgramData\Strongthium\emudhra-security-patch. [y/N]
   ```

   The number counts the certificates it took in on its own as well as the list's. Type `y` and Enter to go on;
   anything else prints `Nothing was changed.` and stops. `--yes` skips the question. With nobody there to answer
   (in a script) and no `--yes`, it stops with an error and changes nothing.
3. Makes the changes, a line each, writing each one down in the backup before making it. What it changes is on
   [On Windows](windows.md#what-revoke-changes) and [On Linux](linux.md#what-revoke-changes).
4. Prints `Checking again:` and looks through the trust stores again, as `scan` does.
5. Says how it went:

| | Exit code |
|---|---|
| `Done: nothing on the list is trusted any more. "emudhra-patch restore" undoes it.` | `0` |
| `<n> still trusted (see above).` | `2` |

An example, on Windows, with most of the lines cut out:

```
Distrust these 13 certificates on this computer? A backup goes to C:\ProgramData\Strongthium\emudhra-security-patch. [y/N] y

  removed from LocalMachine\AuthRoot: emSign ECC Root CA - G3
  distrusted in LocalMachine\Disallowed: EM EV TLS CA – G3B (eMudhra BV)
  distrusted in LocalMachine\Disallowed: EM OV TLS CA – G3B (eMudhra BV)
  distrusted in LocalMachine\Disallowed: emSign ECC Root CA - G3

Checking again:

[distrusted] emSign ECC Root CA - G3 (Root, trust anchor)
             SHA-256 86A1ECBA089C4A8D3BBE2734C612BA341D813E043CF9E8A862CD5C57A36BBE6B
             distrusted in LocalMachine\Disallowed

Done: nothing on the list is trusted any more. "emudhra-patch restore" undoes it.
```

## Running it again

Running `revoke` again changes only what has changed since: a certificate already distrusted isn't distrusted twice,
and one already taken out isn't there to take out. So it can be run on a schedule with `--yes`, or after [`update`](installing-and-updating.md#update)
brings a new list. Each run adds its changes to the same backup, and `restore` undoes them all.

## When something is still trusted

`<n> still trusted (see above)` means a certificate is still in a place that trusts it after the changes. The
lines above it say where. On Linux, look for a `WARNING:` line among the changes: the bundles couldn't be built
again, or a certificate of the distribution's has no blocklist to overrule it. See
[Messages and exit codes](messages.md#revoke-on-linux).

← [scan](scan.md) · [Contents](README.md) · [The backup and restore](backup-and-restore.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
