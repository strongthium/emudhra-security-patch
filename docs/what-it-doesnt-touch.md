<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › What it doesn't touch

# What it doesn't touch

`emudhra-patch` changes the trust stores of the system itself, and nothing else. Programs that keep their own list
of roots go on trusting what's in it.

## Programs with their own roots

- Firefox and Thunderbird, unless they use the system's roots (`security.enterprise_roots.enabled` on Windows)
- Java's `cacerts`, unless it's built from the system's (Debian, Fedora and Arch do that)
- Linux NSS databases (`~/.pki/nssdb`)
- Docker images and other containers, which have trust stores of their own

To distrust the certificates there too, use that program's own way of managing its roots. The PEMs in `certs/` are
the certificates themselves, ready to import.

## Other users

On Windows it reads and changes the machine's stores and the stores of the user it runs as: nobody else's. Run as
administrator with another account's name and password, that user is the other account. With `--current-user`, only
that user's.

On Linux it reads and changes only the system's directories listed on [On Linux](linux.md#where-it-looks): nothing in
anyone's home directory.

## Other certificates

Only the certificates of the list, and those it takes in on its own because they have the same key as one of them
or issued one (see [What it distrusts](what-it-distrusts.md#what-it-adds-on-its-own)). Every other certificate stays
where it is. On Linux, a file of the machine's own that holds a listed certificate among others is written again with
the others left in it.

## What was distrusted already

A certificate that was in **Disallowed** or a blocklist before `revoke` ran is left there. `revoke` doesn't put it
there twice, and `restore` doesn't take it out.

## The network

`scan`, `revoke` and `restore` connect to nothing. Only `about` and `update` do, to GitHub: see
[Installing, about and update](installing-and-updating.md#what-it-sends).

## On an update

[`update`](installing-and-updating.md#update) replaces the program and the list beside it, and leaves the trust
stores and the backup alone: what was distrusted stays distrusted. A list given with `--list` somewhere else isn't
replaced.

← [On Linux](linux.md) · [Contents](README.md) · [Changing the list](changing-the-list.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
