# emudhra-security-patch

`emudhra-patch` finds eMudhra's G3B and G3C intermediate CA certificates in a computer's trust stores, along with
the roots that issued them, and distrusts them. It runs on Windows and Linux and is one file with no runtime to
install. The certificates it looks for are in [`badcerts.txt`](badcerts.txt), a text file you can edit.

## What it distrusts

| Certificate | Kind |
|---|---|
| EM EV / OV / DV TLS CA – G3B (eMudhra BV) | TLS intermediates |
| EM EV / OV / DV TLS CA – G3C (eMudhra Inc) | TLS intermediates |
| EM OV CS CA – G3B (eMudhra BV), EM OV CS CA – G3C (eMudhra Inc) | code signing intermediates |
| emSign Root TLS CA - G3, self-signed and cross-signed | the root of the six TLS intermediates |
| emSign Root CS CA - G3, self-signed and cross-signed | the root of the two code signing intermediates |
| emSign ECC Root CA - G3 | the root that cross-signed both, and the one Windows, Mozilla, Apple and the Linux distributions trust |

Distrusting **emSign ECC Root CA - G3** distrusts everything eMudhra issued under it, not only the certificates
above. To keep it, put `#` in front of its line in `badcerts.txt`.

It also distrusts any certificate it finds that has the same key as one on the list (another issue or
cross-signature of the same CA), and the certificates that issued those, up to the root.

## Using it

Download the build for your system from [Releases](../../releases), unpack it, and run:

```
emudhra-patch scan       # where each certificate is; changes nothing
emudhra-patch revoke     # distrusts them (as administrator on Windows, root on Linux)
emudhra-patch restore    # undoes revoke from the backup it made
emudhra-patch about      # who makes this, and whether there's a newer version
emudhra-patch update     # installs the newer version and its list: it asks first
```

`scan` exits with 2 when something on the list is still trusted, so a script or a monitoring check can use it.
`revoke --yes` runs without asking. `--list <file>` uses another list; the PEMs are read from `certs/` beside it.
On Windows, `--current-user` changes only your own certificate stores and needs no administrator.

`about` and `update` ask `api.github.com` for the latest version, and nothing else ever connects to anything. `update`
downloads the release for this system, checks it against the release's `SHA256SUMS`, and replaces the program, and
`badcerts.txt` and `certs/` when they're beside it. A list that was edited is kept as `badcerts.txt.old`. Nothing it
distrusted is touched: run `scan` afterwards to see the new list against this computer. The checksums come with the
release, so they catch a damaged download, not a tampered release. `update --yes` runs without asking.
### On Windows

Each certificate is removed from every store that trusts it: Root, AuthRoot, CA, TrustedPublisher, TrustedPeople
and My, for the machine and for the current user. All of the listed certificates are then added to the machine's
**Disallowed** store, which Windows checks before any other store. That holds even for a root this computer doesn't
have yet: Windows Update fetches trusted roots only when they're first needed, and once a root is in Disallowed
Windows won't trust it after it arrives.

Removing a root from the current user's Root store makes Windows ask for confirmation. Answer yes.

### On Linux

| Distribution | What changes |
|---|---|
| Debian, Ubuntu | the root is deselected in `/etc/ca-certificates.conf` (a `!` in front of it), then `update-ca-certificates` |
| Fedora, RHEL, Arch, openSUSE | every listed certificate goes into the p11-kit blocklist (`/etc/pki/ca-trust/source/blocklist`, `/etc/ca-certificates/trust-source/blocklist`, `/etc/pki/trust/blacklist`), then `update-ca-trust` |
| all of them | a listed certificate in the machine's own anchors (`/usr/local/share/ca-certificates`, `.../source/anchors`) is taken out of its file |

### What it doesn't touch

These keep their own lists of roots:

- Firefox and Thunderbird, unless they use the system's roots (`security.enterprise_roots.enabled` on Windows)
- Java's `cacerts`, unless it's built from the system's (Debian, Fedora and Arch do that)
- Linux NSS databases (`~/.pki/nssdb`)
- Docker images and other containers, which have trust stores of their own

### The backup

`revoke` keeps a copy of everything it removes or changes, plus `manifest.txt`, a list of each change. `restore`
undoes the changes, last one first. The backup is in `%ProgramData%\Strongthium\emudhra-security-patch`
(`%LocalAppData%\...` with `--current-user`) on Windows and in `/var/lib/emudhra-security-patch` on Linux.

## Changing the list

Each line of `badcerts.txt` is one certificate, with its fields separated by tabs:

```
Name	Kind	Reference	Serial	SHA-256 fingerprint
```

Certificates are matched by their SHA-256 fingerprint. Put a certificate's PEM in `certs/<SHA-256>.pem` and it is
distrusted on computers that don't have it yet. Without a PEM, it's only removed where it's found. `emudhra-patch`
checks that each PEM matches its fingerprint and serial.

## Building

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build src/EmudhraPatch
dotnet publish src/EmudhraPatch -c Release -r win-x64 -o out     # or linux-x64, win-arm64, linux-arm64
```

`master` holds the source. A push to `deploy` (a merge into it counts) runs
[`.github/workflows/release.yml`](.github/workflows/release.yml), which:

1. builds the four platforms;
2. tests revoke and restore on Ubuntu, Fedora, Arch and Windows ([`tests/`](tests));
3. publishes a release with the files and their `SHA256SUMS`.

## License

MIT, see [LICENSE](LICENSE).
