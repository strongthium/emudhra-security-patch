<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › On Windows

# On Windows

On Windows, `emudhra-patch` works on Windows' own certificate stores, the ones Windows and the programs that ask
Windows to check a certificate go by. It takes the listed certificates out of every store that trusts them and puts
them into **Disallowed**, which Windows checks before any other store.

## Running it

Unpack the zip, open a terminal in its folder, and:

```
emudhra-patch scan
```

`scan` needs no administrator. `revoke` and `restore` change the machine's stores, so they need a terminal opened
with **Run as administrator** (right-click Terminal or PowerShell in the Start menu). Without it they stop, with exit
code `3`:

```
error: revoke changes the machine's certificate stores: run it as administrator (or with --current-user for your own stores only).
```

## Where it looks

The machine's stores (`LocalMachine`) and the current user's (`CurrentUser`), the same seven in each:

| Store | As `certlm.msc` and `certmgr.msc` call it | |
|---|---|---|
| **AuthRoot** | Third-Party Root Certification Authorities | the roots Windows Update brings |
| **Root** | Trusted Root Certification Authorities | |
| **CA** | Intermediate Certification Authorities | |
| **TrustedPublisher** | Trusted Publishers | |
| **TrustedPeople** | Trusted People | |
| **My** | Personal | |
| **Disallowed** | Untrusted Certificates | distrusts what's in it |

`scan` and `revoke` show each place as the location and the store: `LocalMachine\AuthRoot`, `CurrentUser\Root`,
`LocalMachine\Disallowed`. A certificate is **TRUSTED** when it's in one of the first six and in no **Disallowed**
store. The machine's **Root** store also shows what's in **AuthRoot**, and the user's **Root** store shows the
machine's roots, so one root is often listed in two or three places.

## What revoke changes

1. Each certificate of the list (and each it took in on its own) found in one of the six stores that trust is taken
   out of it, after a copy is kept in the backup. The machine's stores go first, and **AuthRoot** before **Root**, so
   that a certificate comes out of the store it's really in. Once it's out of the machine's stores, the views of it in the other stores go with it.

   ```
   removed from LocalMachine\AuthRoot: emSign ECC Root CA - G3
   ```

2. Every certificate of the list is put into the machine's **Disallowed** store, whether this computer has it or not,
   as long as its PEM is in `certs/` (or the computer has it). One that's there already is left as it is.

   ```
   distrusted in LocalMachine\Disallowed: EM EV TLS CA – G3B (eMudhra BV)
   ```

**Disallowed** holds even for a root this computer doesn't have yet. Windows Update fetches trusted roots only when
they're first needed, and once a root is in **Disallowed**, Windows won't trust it after it arrives.

Removing a root from the current user's **Root** store makes Windows ask for confirmation, in a window of its own.
Answer **Yes**.

## Only your own stores

```
emudhra-patch scan --current-user
emudhra-patch revoke --current-user
emudhra-patch restore --current-user
```

With `--current-user`, it reads and changes only the current user's stores, and distrusts in the current user's
**Disallowed** store instead of the machine's. It needs no administrator, and keeps its backup in
`%LocalAppData%\Strongthium\emudhra-security-patch`, so `restore` needs `--current-user` too to find it.

The certificates are then distrusted for that user only, and the machine's stores are left as they are.

## Seeing it for yourself

The machine's **Disallowed** store, in PowerShell:

```
Get-ChildItem Cert:\LocalMachine\Disallowed | Where-Object Subject -match 'emSign|eMudhra'
```

or `certlm.msc` (the machine's) and `certmgr.msc` (yours), under **Untrusted Certificates**.

## The backup

In `%ProgramData%\Strongthium\emudhra-security-patch`: a `.cer` copy of each certificate taken out of a store, and
`manifest.txt`. See [The backup and restore](backup-and-restore.md).

← [The backup and restore](backup-and-restore.md) · [Contents](README.md) · [On Linux](linux.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
