<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › What it distrusts

# What it distrusts

`emudhra-patch` distrusts the certificates listed in [`badcerts.txt`](../badcerts.txt), which sits beside the
program: eMudhra's eight G3B and G3C intermediate CA certificates, the two roots that issued them (each in two
versions), and the root above those. Thirteen lines in all. This page says what each one is, why it is on the list,
and what else the program adds to the list on its own.

## The list

| Certificate | Kind | Reference | Why it's on the list |
|---|---|---|---|
| EM EV TLS CA – G3B (eMudhra BV) | TLS | A012304 | one of the intermediates |
| EM OV TLS CA – G3B (eMudhra BV) | TLS | A012308 | one of the intermediates |
| EM DV TLS CA – G3B (eMudhra BV) | TLS | A012302 | one of the intermediates |
| EM EV TLS CA – G3C (eMudhra Inc) | TLS | A012296 | one of the intermediates |
| EM OV TLS CA – G3C (eMudhra Inc) | TLS | A012300 | one of the intermediates |
| EM DV TLS CA – G3C (eMudhra Inc) | TLS | A012294 | one of the intermediates |
| EM OV CS CA – G3B (eMudhra BV) | Code signing | A012306 | one of the intermediates |
| EM OV CS CA – G3C (eMudhra Inc) | Code signing | A012298 | one of the intermediates |
| emSign Root TLS CA - G3 | Root | self-signed | issued the six TLS intermediates |
| emSign Root TLS CA - G3 | Root | cross-signed by emSign ECC Root CA - G3 | the same root, as the ECC root signed it |
| emSign Root CS CA - G3 | Root | self-signed | issued the two code signing intermediates |
| emSign Root CS CA - G3 | Root | cross-signed by emSign ECC Root CA - G3 | the same root, as the ECC root signed it |
| emSign ECC Root CA - G3 | Root | trust anchor | the root the cross-signatures lead to, and the one Windows, Mozilla, Apple and the Linux distributions trust |

The SHA-256 fingerprint of each is the last field of its line in `badcerts.txt`, and is what the program matches
certificates by. `scan` and `revoke` print each one's fingerprint under its name.

## Why the roots are on it too

A computer trusts an intermediate through the root that issued it, so the list doesn't stop at the intermediates: it
goes on to **emSign Root TLS CA - G3** and **emSign Root CS CA - G3**, which issued them, both versions of each, and
to **emSign ECC Root CA - G3**, which cross-signed both.

**emSign ECC Root CA - G3** is the one Windows, Mozilla, Apple and the Linux distributions trust. Distrusting it distrusts everything eMudhra issued under
it, not only the certificates above. To keep it, put `#` in front of its line in `badcerts.txt` (see
[Changing the list](changing-the-list.md)).

## What it adds on its own

Besides the lines of the list, `scan` and `revoke` take in two more kinds of certificate, from those they find on the
computer:

| | |
|---|---|
| **The same key** | a certificate with the same public key as one on the list: another issue or another cross-signature of the same CA. It's shown as `(same key as <name>)` |
| **The issuer** | a certificate that issued one on the list (its subject is the other's issuer, and their key identifiers agree where both have one). It's shown as `(issued <name>)` |

It keeps looking until nothing more is added, so the issuers of the issuers are taken in too, up to the root. These
are printed after the list's own, under the name the certificate itself gives (its common name).

## Distrusted ahead of time

[`certs/`](../certs), beside the list, holds each certificate itself, as `certs/<SHA-256>.pem`. A certificate with a
PEM there is distrusted even on a computer that doesn't have it yet. Windows fetches the roots it trusts only when
they're first needed, so a root that is missing today can arrive tomorrow; once it's distrusted, it won't be trusted
when it does. All thirteen on the list have their PEM.

A certificate without a PEM is only taken out of the stores where it's found.

[Contents](README.md) · [Installing, about and update](installing-and-updating.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
