<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › Manual

# The emudhra-security-patch manual

Every command of `emudhra-patch`, with what it prints and changes, on Windows and on Linux, and how to undo it.

## The short version

```
# Download the .zip (Windows) or .tar.gz (Linux) for your system from
# https://github.com/strongthium/emudhra-security-patch/releases/latest and unpack it. Then, in its folder:

emudhra-patch scan          # where each certificate is, and whether it's trusted; changes nothing
emudhra-patch revoke        # distrusts them: as administrator on Windows, with sudo on Linux
emudhra-patch restore       # undoes revoke, from its backup
```

On Linux, it's `./emudhra-patch` in that folder, and `sudo ./emudhra-patch revoke`.

## Getting started

- [What it distrusts](what-it-distrusts.md): the thirteen certificates of the list, why each is on it, and what it
  adds on its own
- [Installing, about and update](installing-and-updating.md): what to download, what's in it, and the `about` and
  `update` commands

## The commands

- [The command line](command-line.md): every command and option, the help, and the exit codes
- [scan](scan.md): where each certificate is, and what each line it prints means
- [revoke](revoke.md): distrusting them, step by step
- [The backup and restore](backup-and-restore.md): what `revoke` keeps, where, and how `restore` undoes it

## Windows and Linux

- [On Windows](windows.md): the certificate stores, what `revoke` changes in them, and `--current-user`
- [On Linux](linux.md): OpenSSL, each family of distributions, the files it reads and what `revoke` changes
- [What it doesn't touch](what-it-doesnt-touch.md): programs with roots of their own, other users, other certificates

## Reference

- [Changing the list](changing-the-list.md): `badcerts.txt`, the PEMs in `certs/`, and a list of your own
- [Messages and exit codes](messages.md): everything it can print, and what each one means
- [Building](building.md): building it, the releases, and the tests

## Strongthium open-source tools

The other free, MIT-licensed tools from Strongthium Labs, each built by GitHub Actions from its source at [github.com/strongthium](https://github.com/strongthium):

<table>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/folder-size"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/treemap.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/treemap.svg" width="48" height="48" alt="Strongthium Folder Size icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/folder-size"><b>Strongthium Folder Size</b></a><br>
      <sub><b>Fast disk space analyzer for Windows.</b> Every folder added up, largest first, as a tree, a treemap, the largest files and the space by type.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/health-reminder"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/pill.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/pill.svg" width="48" height="48" alt="Strongthium Health Reminder icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/health-reminder"><b>Strongthium Health Reminder</b></a><br>
      <sub><b>Medication reminder, pill identifier and meal planner for Windows.</b> Insistent reminders of pills, meals and water, with a picture of the very pill to take.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/host-dossier"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/dossier.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/dossier.svg" width="48" height="48" alt="Strongthium Host Dossier icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/host-dossier"><b>Strongthium Host Dossier</b></a><br>
      <sub><b>Domain and IP lookup tool for Windows and Linux.</b> WHOIS, DNS, TLS, open ports and every mail record of a domain or an address, in one report.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/outbound-watch"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/outbound.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/outbound.svg" width="48" height="48" alt="Strongthium Outbound Watch icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/outbound-watch"><b>Strongthium Outbound Watch</b></a><br>
      <sub><b>Outbound connection monitor for Windows.</b> Where every program on your PC connects to, live and as history, and a warning when one goes somewhere new.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/raw-convert"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/camera.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/camera.svg" width="48" height="48" alt="Strongthium RAW Convert icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/raw-convert"><b>Strongthium RAW Convert</b></a><br>
      <sub><b>Batch CR2 to PNG and TIFF converter for Windows.</b> Folders of Canon RAW photos made lossless PNG or TIFF at full size, with the date, camera and GPS kept.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/video-dispatch"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/video.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/video.svg" width="48" height="48" alt="Strongthium Video Dispatch icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/video-dispatch"><b>Strongthium Video Dispatch</b></a><br>
      <sub><b>TikTok and YouTube uploader and video editor for Windows.</b> One video queued to TikTok, YouTube and Shorts, cut and trimmed with Windows' own tools.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://github.com/strongthium/webdav-server"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/webdav.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/webdav.svg" width="48" height="48" alt="Strongthium WebDAV Server icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://github.com/strongthium/webdav-server"><b>Strongthium WebDAV Server</b></a><br>
      <sub><b>Self-hosted WebDAV server for Linux and Windows.</b> Files by user or by path, Nextcloud-compatible, with bans and IP, ASN and country whitelists.</sub>
    </td>
    <td></td>
    <td></td>
  </tr>
</table>

## More from Strongthium

Strongthium Labs also makes software for running your own servers and Windows PCs, for talking to your customers, and for billing your hours, at [strongthium.com](https://strongthium.com):

<table>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/linux-server-firewall"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/guard.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/guard.svg" width="48" height="48" alt="Strongthium Guard icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/linux-server-firewall"><b>Strongthium Guard</b></a><br>
      <sub><b>Linux server firewall.</b> Locks down your Linux servers, reads their logs and shuts out whoever attacks them, within seconds.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/self-hosted-mail-server"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/mail.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/mail.svg" width="48" height="48" alt="Strongthium Mail icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/self-hosted-mail-server"><b>Strongthium Mail</b></a><br>
      <sub><b>Self-hosted mail server for Linux and Windows.</b> Receives, checks and sends your mail, and tells you exactly why it refused what it refused.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/self-hosted-live-chat"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/chat.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/chat.svg" width="48" height="48" alt="Strongthium Chat icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/self-hosted-live-chat"><b>Strongthium Chat</b></a><br>
      <sub><b>Self-hosted live chat for Linux and Windows.</b> A chat on your websites and text messages in one inbox, answered from Windows or a browser.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/windows-ssh-client"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/remote.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/remote.svg" width="48" height="48" alt="Strongthium Remote icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/windows-ssh-client"><b>Strongthium Remote</b></a><br>
      <sub><b>Windows SSH client.</b> SSH terminal, file manager, editor, server task manager, database client and remote desktop in tabs, in one program.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/twilio-sms-desktop-app"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/sms.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/sms.svg" width="48" height="48" alt="Strongthium SMS icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/twilio-sms-desktop-app"><b>Strongthium SMS</b></a><br>
      <sub><b>Twilio text messages for Windows.</b> Sends and receives your own Twilio account's texts from Windows, and blocks senders by number or by country.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/time-tracking-invoicing-software"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/work.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/work.svg" width="48" height="48" alt="Strongthium Work icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/time-tracking-invoicing-software"><b>Strongthium Work</b></a><br>
      <sub><b>Time tracking and invoicing for Windows.</b> Logs the hours you work for each client, and turns them into invoices e-mailed as PDF, alone or with a team.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/windows-registry-editor"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/registry.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/registry.svg" width="48" height="48" alt="Strongthium Registry icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/windows-registry-editor"><b>Strongthium Registry</b></a><br>
      <sub><b>Windows registry editor.</b> Shows what each key and value means, finds any key in seconds, and backs up every change before it's made.</sub>
    </td>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/cfml-server"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/dotcf.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/dotcf.svg" width="48" height="48" alt="Strongthium DotCF icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/cfml-server"><b>Strongthium DotCF</b></a><br>
      <sub><b>CFML server for Windows and Linux.</b> Runs applications written for Adobe ColdFusion and Lucee on .NET, web server and all, in one program.</sub>
    </td>
  </tr>
  <tr>
    <td width="64" align="center" valign="top">
      <a href="https://strongthium.com/video-conferencing-dial-up-modem"><picture><source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/dark/convey.svg"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/icons/light/convey.svg" width="48" height="48" alt="Strongthium Convey icon"></picture></a>
    </td>
    <td valign="top">
      <a href="https://strongthium.com/video-conferencing-dial-up-modem"><b>Strongthium Convey</b></a><br>
      <sub><b>Video conferencing for Windows.</b> Video, sound and text chat between Windows PCs, over the Internet, a dial-up modem or a cable.</sub>
    </td>
    <td></td>
    <td></td>
  </tr>
</table>

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
