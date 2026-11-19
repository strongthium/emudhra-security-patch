<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › scan

# scan

```
emudhra-patch scan
emudhra-patch
```

Reads the list, looks through every trust store of the computer, and says where each certificate of the list is and
whether the computer trusts it. It changes nothing, and needs no administrator or root. It's what the program does
when it's given no command.

## What it prints

An example, on Windows, with most of the certificates cut out:

```
13 certificates in C:\Tools\emudhra-patch\badcerts.txt

[not here  ] EM EV TLS CA – G3B (eMudhra BV) (TLS, A012304)
             SHA-256 E7568FE6E12331F4FD7DE61DA4C7559D0F8EB192A218D4741A3BA75184333838

[not here  ] EM OV TLS CA – G3B (eMudhra BV) (TLS, A012308)
             SHA-256 98B896625C0C230ADEEC3EB2CF6AAD5001ECF9156A68644E82374CE5833BC25B

[TRUSTED   ] emSign ECC Root CA - G3 (Root, trust anchor)
             SHA-256 86A1ECBA089C4A8D3BBE2734C612BA341D813E043CF9E8A862CD5C57A36BBE6B
             in LocalMachine\AuthRoot
             in LocalMachine\Root
             in CurrentUser\Root

1 trusted. Run "emudhra-patch revoke" as administrator to distrust them; it also distrusts the ones with a PEM that are not here yet.
```

And the same root on Ubuntu:

```
[TRUSTED   ] emSign ECC Root CA - G3 (Root, trust anchor)
             SHA-256 86A1ECBA089C4A8D3BBE2734C612BA341D813E043CF9E8A862CD5C57A36BBE6B
             in /usr/share/ca-certificates/mozilla/emSign_ECC_Root_CA_-_G3.crt
             in /etc/ssl/certs/ca-certificates.crt
```

| | |
|---|---|
| **`13 certificates in ...`** | how many certificates the list has, and which list it read. A line that's there twice counts once |
| **`note:`** | under it, a line of the list that has no PEM in `certs/`, or whose serial isn't the one its PEM has. See [Messages and exit codes](messages.md#reading-the-list) |
| **`[TRUSTED   ]`** | the computer trusts it: it's in a store that trusts it, and nothing distrusts it |
| **`[distrusted]`** | it's on the computer, but distrusted |
| **`[not here  ]`** | it isn't on the computer at all |
| **the name** | the certificate's name, from the list |
| **`(TLS, A012304)`** | its kind and reference, from the list. For a certificate the list doesn't have but that belongs with one that it has, why it was taken in: `(same key as <name>)` or `(issued <name>)`. See [What it distrusts](what-it-distrusts.md#what-it-adds-on-its-own) |
| **`SHA-256`** | its fingerprint, what it's matched by |
| **`in ...`** | each place it was found that trusts it: a certificate store on Windows, a file on Linux |
| **`distrusted in ...`** | each place it was found that distrusts it: a **Disallowed** store on Windows, a blocklist on Linux (or, on Debian and Ubuntu, a root that `/etc/ca-certificates.conf` doesn't select) |

The certificates are in the order of the list, then the ones it took in on its own.

## The last line

| | Exit code |
|---|---|
| `Nothing on the list is trusted on this computer.` | `0` |
| `<n> trusted. Run "emudhra-patch revoke" as administrator to distrust them; it also distrusts the ones with a PEM that are not here yet.` (`as root` on Linux) | `2` |

The exit code makes `scan` a check for a script or a monitoring system: `2` means something on the list is trusted.
See [The command line](command-line.md#exit-codes).

## Where it looks

On Windows, every certificate store that trusts (**AuthRoot**, **Root**, **CA**, **TrustedPublisher**,
**TrustedPeople**, **My**) and **Disallowed**, the machine's and the current user's: see [On Windows](windows.md).
With `--current-user`, only the current user's.

On Linux, the distribution's CA certificates, the machine's own, the blocklists and the bundles built from them: see
[On Linux](linux.md#where-it-looks). A file it isn't allowed to read is left out.

## Trusted, on Linux

On Linux a certificate counts as **TRUSTED** when one of the bundles programs read has it, or when one of the sources
those bundles are built from has it and no blocklist overrules it: the next rebuild would put it in a bundle again.
So a certificate added to a blocklist by hand is still **TRUSTED** until the bundles are built again
(`update-ca-certificates` or `update-ca-trust`). `revoke` builds them again itself.

## Another list

```
emudhra-patch scan --list D:\lists\mine.txt
```

reads that list instead, and the PEMs in `D:\lists\certs\`. See [Changing the list](changing-the-list.md).

← [The command line](command-line.md) · [Contents](README.md) · [revoke](revoke.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
