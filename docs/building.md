<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › Building

# Building

`emudhra-patch` is a .NET 10 program, one project in [`src/EmudhraPatch`](../src/EmudhraPatch), with no packages
beyond .NET itself.

## On your own computer

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build src/EmudhraPatch
```

builds it into `src/EmudhraPatch/bin/Debug/net10.0/`, with `badcerts.txt` and `certs/` copied beside it. It runs
there with .NET 10 installed.

```
dotnet publish src/EmudhraPatch -c Release -r win-x64 -o out
```

makes the program as it's released, in `out/`, for one system: `win-x64`, `win-arm64`, `linux-x64` or
`linux-arm64`. Given a system, the build is:

| | |
|---|---|
| **Self-contained** | .NET goes into the program, so nothing has to be installed to run it |
| **One file** | `emudhra-patch.exe` or `emudhra-patch`, compressed, with no `.pdb` |
| **Trimmed** | the parts of .NET it doesn't use are left out: about a third of the size |
| **No ICU** | it compares nothing by culture, so a minimal Linux without `libicu` runs it too |
| **The list beside it** | `badcerts.txt` and `certs/` stay files beside the program, not inside it, so the list can be changed without a build |

The version is in [`VERSION`](../VERSION), at the top of the repository: four numbers.

## Releases

`master` holds the source, and nothing is built from it. A push to `deploy` (a merge into it counts) runs
[`.github/workflows/release.yml`](../.github/workflows/release.yml), which:

1. builds the four systems, `win-x64`, `win-arm64`, `linux-x64` and `linux-arm64`, and puts `README.md` and
   `LICENSE` beside each;
2. tests `scan`, `revoke` and `restore` on Ubuntu (`update-ca-certificates`), on Fedora and Arch
   (`update-ca-trust`), and on Windows. On Fedora, when OpenSSL 3 isn't installed, it also checks that the program
   says to install it;
3. publishes the release `v<VERSION>`, with a `.zip` for each Windows build, a `.tar.gz` for each Linux one, and
   their `SHA256SUMS`.

A version is released once: a push to `deploy` with a version that's out already fails, and says to raise the
version in `VERSION`.

## The tests

[`tests/linux.sh`](../tests/linux.sh) and [`tests/windows.ps1`](../tests/windows.ps1) take the folder of a built
program. Each adds **emSign Root TLS CA - G3** to the computer's trusted roots (on Linux, as one of the machine's own,
beside the distribution's **emSign ECC Root CA - G3**), and then checks that:

| | |
|---|---|
| `scan` | finds it trusted, and exits with `2` |
| `revoke --yes` | takes it out and exits with `0`. On Windows, every listed certificate is in the machine's **Disallowed** store; on Linux, the bundle has changed |
| `scan` | finds nothing trusted, and exits with `0` |
| `restore` | puts it back. On Windows, **Disallowed** is as it was; on Linux, the bundle is the same to the byte |
| `scan` | finds it trusted again (Linux) |

```
sudo sh tests/linux.sh "$PWD/out"
pwsh ./tests/windows.ps1 -App out
```

They change the trust stores of the computer they run on, and need root or administrator: the workflow runs them on
GitHub's machines.

← [Messages and exit codes](messages.md) · [Contents](README.md)

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
