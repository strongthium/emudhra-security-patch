<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › On Linux

# On Linux

On Linux, the CA certificates programs trust are built into bundles (`/etc/ssl/certs/ca-certificates.crt`,
`/etc/pki/ca-trust/extracted/...`) from a few sources: the distribution's own package of them, and the machine's own
additions. `emudhra-patch` changes the sources, and then builds the bundles again with the distribution's own tool,
so the change lasts when the bundles are next built.

## OpenSSL

It needs OpenSSL 3 (`libssl.so.3`), which .NET reads certificates with. Without it, it says what to install and
stops, with exit code `1`, before doing anything else:

```
error: this needs OpenSSL 3 (libssl.so.3), which .NET reads certificates with, and it isn't installed.
Fedora 45 and later come with OpenSSL 4, which .NET can't use yet. Install OpenSSL 3 beside it:
  sudo dnf install openssl3-libs
On other distributions, install their OpenSSL 3 library (libssl3 on Debian and Ubuntu).
```

## Running it

```
tar -xzf emudhra-patch-<version>-linux-x64.tar.gz
cd emudhra-patch-<version>-linux-x64
./emudhra-patch scan
sudo ./emudhra-patch revoke
```

`scan` needs no root. `revoke` and `restore` change the system's CA certificates, so they're run as root. Without
it, they stop, with exit code `3`:

```
error: revoke changes the system's CA certificates: run it as root (sudo).
```

## The distributions

| Distribution | What `revoke` changes |
|---|---|
| Debian, Ubuntu | the distribution's root is deselected in `/etc/ca-certificates.conf` (a `!` in front of its line), then `update-ca-certificates` |
| Fedora, RHEL, Arch, openSUSE | every certificate of the list goes into the p11-kit blocklist, which overrules every other source, then `update-ca-trust` |
| all of them | a certificate of the list among the machine's own additions is taken out of its file |

The blocklists it uses, where their parent directory exists:

| | |
|---|---|
| `/etc/pki/ca-trust/source/blocklist` | Fedora, RHEL |
| `/etc/ca-certificates/trust-source/blocklist` | Arch |
| `/etc/pki/trust/blacklist` | openSUSE |

Older systems call the directory `blacklist`; when a `blacklist` is there and no `blocklist`, it uses that.

## Where it looks

Every file under these, in every folder below them, ending `.pem`, `.crt`, `.cer`, `.der`, `.p11-kit`, `.trust` or
with no extension:

| | | |
|---|---|---|
| The machine's own | `/usr/local/share/ca-certificates`, `/etc/pki/ca-trust/source`, `/etc/ca-certificates/trust-source`, `/etc/pki/trust` | trusted, unless a blocklist overrules them |
| The blocklists | `blocklist` and `blacklist` folders in those | distrusted |
| The distribution's | `/usr/share/ca-certificates`, `/usr/share/pki/ca-trust-source`, `/usr/share/pki/trust` | trusted, unless a blocklist overrules them. On Debian and Ubuntu, only when `/etc/ca-certificates.conf` selects them |
| The bundles | `/etc/ssl/certs`, `/etc/ssl/ca-bundle.pem`, `/etc/pki/ca-trust/extracted`, `/etc/pki/tls/certs`, `/etc/ca-certificates/extracted`, `/var/lib/ca-certificates` | trusted: these are what programs read |

A file can hold one certificate or many: PEM, a bundle of PEMs, a p11-kit file, or one DER certificate. It skips
links (those in `/etc/ssl/certs` point at files it reads where they are), files over 64 MB, and files it isn't
allowed to read.

A certificate is **TRUSTED** when a bundle has it, or when a source has it and no blocklist overrules it, since the
next rebuild would put it in a bundle again.

## What revoke changes

In this order, each change written down in the backup first:

1. **The machine's own.** Each file among the machine's own that holds a certificate of the list is copied to the
   backup, then written again without it, or deleted when nothing else was in it. A `.p11-kit` file is left as it is:
   the blocklist overrules it.

   ```
   removed from /usr/local/share/ca-certificates/bundle.crt
   deleted /usr/local/share/ca-certificates/emsign-root.crt (nothing else was in it)
   left in /etc/pki/ca-trust/source/anchors/extra.p11-kit (a p11-kit file; the blocklist overrules it)
   ```

2. **Debian and Ubuntu.** Each distribution's root of the list that `/etc/ca-certificates.conf` selects gets a `!` in
   front of its line there, which deselects it.

   ```
   deselected in /etc/ca-certificates.conf: mozilla/emSign_ECC_Root_CA_-_G3.crt
   ```

3. **The blocklists.** Into each blocklist, every certificate of the list whose PEM is in `certs/` (or that the
   computer has), whether the computer has it or not, as `emudhra-security-patch-<the first 16 digits of its
   SHA-256>.pem`. One that's there already is left as it is.

   ```
   distrusted in /etc/pki/ca-trust/source/blocklist: emSign ECC Root CA - G3
   ```

4. **The bundles.** It runs `update-ca-certificates`, and `update-ca-trust extract`, whichever of the two is
   installed (both, when both are), and shows what each one prints, after its name.

   ```
     update-ca-certificates: Updating certificates in /etc/ssl/certs...
   rebuilt the bundles: update-ca-certificates
   ```

A certificate that isn't on the computer is distrusted ahead of time only through a blocklist. Debian and Ubuntu
deselect the roots they have instead, so there a certificate of the list that isn't on the computer isn't distrusted
ahead of time, unless the system has one of the blocklists' parent directories as well.

## Warnings

A `WARNING:` among the changes means part of it didn't take:

| | |
|---|---|
| `WARNING: <file> is the distribution's and this system has no p11-kit blocklist to overrule it` | a certificate of the list is in one of the distribution's files, and there's no blocklist to overrule it, so it stays trusted |
| `WARNING: <tool> failed with exit code <n>` | the tool that builds the bundles failed: its own lines above say why. The bundles may still hold the certificates |
| `WARNING: neither update-ca-certificates nor update-ca-trust is installed: the bundles were not rebuilt` | the sources are changed, but the bundles programs read still hold the certificates |

`Checking again:` after them shows what's still trusted, and where.

## The backup

In `/var/lib/emudhra-security-patch`: a copy of each file of the machine's own it changed, and `manifest.txt`. See
[The backup and restore](backup-and-restore.md). `restore` puts them back, deletes the blocklist files it made,
selects the roots again and builds the bundles again.

← [On Windows](windows.md) · [Contents](README.md) · [What it doesn't touch](what-it-doesnt-touch.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
