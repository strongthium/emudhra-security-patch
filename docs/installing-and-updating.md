<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › Installing, about and update

# Installing, about and update

`emudhra-patch` is one program file with its list beside it. There is nothing to install: download it, unpack it,
and run it from that folder. `about` says who makes it and whether there's a newer version, and `update` installs one.

## Downloading it

The [latest release](https://github.com/strongthium/emudhra-security-patch/releases/latest) has one archive for each
system, and `SHA256SUMS`, the SHA-256 checksum of each:

| File | For |
|---|---|
| `emudhra-patch-<version>-win-x64.zip` | Windows on x64 PCs |
| `emudhra-patch-<version>-win-arm64.zip` | Windows on ARM |
| `emudhra-patch-<version>-linux-x64.tar.gz` | Linux on x64 |
| `emudhra-patch-<version>-linux-arm64.tar.gz` | Linux on ARM |

Each one unpacks to a folder of the same name, holding:

| | |
|---|---|
| `emudhra-patch.exe` (Windows), `emudhra-patch` (Linux) | the program. .NET is built into it, so there is no runtime to install |
| `badcerts.txt` | the list of certificates to distrust: see [What it distrusts](what-it-distrusts.md) |
| `certs/` | the certificates themselves, one PEM each |
| `README.md`, `LICENSE` | |

Keep the four together. The program looks for `badcerts.txt` beside itself first, then in the current directory, and
reads the PEMs from the `certs/` folder beside the list.

On Windows, unpack the zip with Explorer (**Extract All**) and open a terminal in the folder. On Linux:

```
tar -xzf emudhra-patch-<version>-linux-x64.tar.gz
cd emudhra-patch-<version>-linux-x64
./emudhra-patch scan
```

On Linux it needs OpenSSL 3 (`libssl.so.3`), which .NET reads certificates with: see [On Linux](linux.md#openssl).

## about

```
emudhra-patch about
```

Prints the program's name and version (`Strongthium eMudhra Security Patch` and four numbers), that it's free and open
source under the MIT license, with the address of its source, and then:

```
Strongthium Labs makes small, focused software that does one job well, for the people who keep
machines running. Everyone else is centralizing and harvesting data. We make software that runs on
your own machines instead: your data and your keys never pass through us, and everything it does is
shown, with the reason why.

More from Strongthium, each with a free edition and no time limit:

  Strongthium Guard     a firewall for Linux servers     https://strongthium.com/linux-server-firewall
  Strongthium Mail      a mail server of your own        https://strongthium.com/self-hosted-mail-server
  Strongthium Chat      live chat you host yourself      https://strongthium.com/self-hosted-live-chat
  Strongthium Remote    an SSH client for Windows        https://strongthium.com/windows-ssh-client
  Strongthium SMS       your Twilio texts on Windows     https://strongthium.com/twilio-sms-desktop-app
  Strongthium Work      time tracking and invoicing      https://strongthium.com/time-tracking-invoicing-software
  Strongthium Registry  a Windows registry editor        https://strongthium.com/windows-registry-editor
  Strongthium DotCF     a CFML server on .NET            https://strongthium.com/cfml-server
  Strongthium Convey    video conferencing for Windows   https://strongthium.com/video-conferencing-dial-up-modem

Website   https://strongthium.com
Our tools https://github.com/strongthium
Security  https://github.com/strongthium/.github/blob/main/SECURITY.md
```

Then it asks GitHub for the latest version (`Asking GitHub for the latest version...`) and says one of:

| | |
|---|---|
| `You have the latest version.` | |
| `This is newer than the latest release, <version>.` | a build of your own, ahead of the releases |
| `Version <version> is out. You have <version>. What's new: <address>` | the address is the release's page. When someone is there to answer, it asks `Update to <version> now?`, as `update` does |

When nobody is there to answer (its input is a file or another program, as in a script), `about` only says that a
newer version is out, and installs nothing, unless it is given `--yes`, which installs it without asking.

## update

```
emudhra-patch update
emudhra-patch update --yes
```

Asks GitHub for the latest version, as `about` does, and when there's a newer one, asks:

```
Update to <version> now? It's checked against the release's checksums first. [y/N]
```

Type `y` and Enter to install it; anything else leaves everything as it was. `--yes` installs it without asking. With
nobody there to answer (in a script) and no `--yes`, `update` stops with an error instead of waiting.

To install it, `update`:

1. downloads the release's `SHA256SUMS`, and the archive for this system (`emudhra-patch-<version>-<platform>.zip` or
   `.tar.gz`);
2. checks the archive against its line in `SHA256SUMS`. If they don't match, nothing is changed;
3. when `badcerts.txt` is beside the program, puts the new release's list in its place, and copies the new release's
   PEMs into `certs/`. When the list beside the program isn't the same as the new one (an edited list, or an older
   one), the one that was there is kept as `badcerts.txt.old`;
4. puts the new program in the place of the running one. On Windows the old one is renamed `emudhra-patch.exe.old`
   and deleted the next time the program starts.

When it's done:

```
Updated to <version>. Certificates it distrusted stay distrusted; run "emudhra-patch scan" to see the new list against this computer.
```

Nothing it distrusted before is touched. A new version is mostly a new list, so run [`scan`](scan.md) afterwards, and
[`revoke`](revoke.md) when it shows something trusted.

The checksums come with the release, so they catch a damaged download, not a tampered release.

When the program is in a folder you can't change (`C:\Program Files`, `/usr/local/bin`), run `update` as
administrator or with `sudo`. Otherwise it says the folder needs an administrator, and changes nothing.

## What it sends

`about` and `update` ask `https://api.github.com/repos/strongthium/emudhra-security-patch/releases/latest` for the
latest release, saying only that it's `emudhra-patch` and its version. `update` then downloads that release's
files from github.com. Nothing else the program does connects to anything: `scan`, `revoke` and `restore` work
offline.

It waits up to 20 seconds for GitHub's answer; a download takes as long as it takes. When GitHub has had too many
questions from this address, `about` and `update` say so: try again in an hour.

The errors `about` and `update` can show are on [Messages and exit codes](messages.md#about-and-update).

← [What it distrusts](what-it-distrusts.md) · [Contents](README.md) · [The command line](command-line.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
