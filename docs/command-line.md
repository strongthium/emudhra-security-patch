<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › The command line

# The command line

`emudhra-patch` is run from a terminal: a command, and options after it or before it, in any order. With no command,
it runs `scan`.

```
emudhra-patch [scan | revoke | restore | about | update] [--list <file>] [--current-user] [--yes]
```

## The commands

| | | Needs |
|---|---|---|
| [`scan`](scan.md) | says where each certificate of the list is, and whether this computer trusts it. Changes nothing. The command it runs when none is given | nothing |
| [`revoke`](revoke.md) | takes the certificates out of the stores that trust them and distrusts them, after asking. Keeps a backup first | administrator on Windows (not with `--current-user`), root on Linux |
| [`restore`](backup-and-restore.md) | undoes what `revoke` did, from its backup, the last change first | the same as `revoke` |
| [`about`](installing-and-updating.md#about) | who makes it, its version, and whether there's a newer one (GitHub is asked) | nothing |
| [`update`](installing-and-updating.md#update) | installs the newer version and its list, if there is one, after asking | the right to change the program's folder |
| `help`, `--help`, `-h`, `/?` | prints the help below and stops | nothing |

When more than one command is given, the last one counts.

## The options

| | |
|---|---|
| `--list <file>` | the list to use, instead of `badcerts.txt` beside the program (or, when there's none there, in the current directory). The PEMs are read from the `certs/` folder beside the file given. `scan` and `revoke` use it; `restore`, `about` and `update` don't. See [Changing the list](changing-the-list.md) |
| `--current-user` | Windows: reads and changes only the current user's certificate stores, and distrusts in the current user's **Disallowed** store, not the machine's. `revoke` and `restore` then need no administrator, and the backup is the user's own. See [On Windows](windows.md#only-your-own-stores). On Linux it changes nothing |
| `--yes`, `-y` | `revoke` and `update` without asking first. Without it, they ask, and stop with an error when nobody is there to answer. `about --yes` installs a newer version without asking, as `update --yes` does |

Anything else stops it before it does anything, with `unknown argument:`, the argument, and the help. So does
`--list` with no file after it.

## The help

`emudhra-patch --help` prints:

```
emudhra-patch: finds the certificates of badcerts.txt in this computer's trust stores and distrusts them,
and the roots that issued them.

  emudhra-patch [scan]      says where each certificate is, and changes nothing
  emudhra-patch revoke      takes them out of the stores that trust them and distrusts them (administrator / root)
  emudhra-patch restore     undoes what revoke did, from the backup it made
  emudhra-patch about       who makes this, and whether there's a newer version (GitHub is asked)
  emudhra-patch update      installs the newer version and its list, if there is one: it asks first

  --list <file>    the list to use (default: badcerts.txt beside the program, then in the current directory);
                    the PEMs are read from certs/ beside it
  --current-user    Windows: only the current user's stores, which needs no administrator
  --yes             revoke or update without asking first

Exit codes: 0 nothing on the list is trusted (any more), 1 error, 2 something on the list is still trusted,
3 not run as administrator / root.
```

## Exit codes

| | |
|---|---|
| `0` | nothing on the list is trusted (`scan`), or nothing is any more (`revoke`). Also: `revoke` answered with no, `restore` done or with nothing to restore, `about` and `update` without an error, and the help |
| `1` | an error: an unknown argument, a list or PEM that can't be read, no answer to a question when nobody is there, no OpenSSL on Linux, or GitHub that can't be reached |
| `2` | something on the list is still trusted: `scan` found one, or `revoke` couldn't distrust them all |
| `3` | `revoke` or `restore` not run as administrator (Windows) or root (Linux) |

`scan`'s `2` is meant for scripts and monitoring checks: run it every so often, and act when it's not `0`.

```
emudhra-patch scan > /dev/null
if [ $? -eq 2 ]; then echo "a certificate on the list is trusted"; fi
```

Every message and when it's shown is on [Messages and exit codes](messages.md).

## In scripts

`revoke` and `update` ask before they change anything. In a script, a scheduled task or a deployment tool, where
nobody is there to answer, give them `--yes`:

```
emudhra-patch revoke --yes
```

On Windows, run it as administrator; on Linux, as root.

← [Installing, about and update](installing-and-updating.md) · [Contents](README.md) · [scan](scan.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
