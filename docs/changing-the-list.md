<p align="center">
  <a href="https://strongthium.com" title="Strongthium Labs"><img src="https://raw.githubusercontent.com/strongthium/.github/main/profile/banner-2200x440.png" alt="Strongthium Labs: Take back control of your internet."></a>
</p>

[emudhra-security-patch](../README.md) › [Manual](README.md) › Changing the list

# Changing the list

The certificates `emudhra-patch` distrusts are in `badcerts.txt`, a text file beside the program, and the
certificates themselves in `certs/`, beside the list. Both can be changed without building anything: `scan` and
`revoke` read them each time they run.

## badcerts.txt

One certificate a line, its fields separated by tabs (two or more spaces do too):

```
Name	Kind	Reference	Serial	SHA-256 fingerprint
```

For example:

```
EM EV TLS CA – G3B (eMudhra BV)	TLS	A012304	c7c41efe92d825f05eae841a4a82c1	E7568FE6E12331F4FD7DE61DA4C7559D0F8EB192A218D4741A3BA75184333838
```

| | |
|---|---|
| **Name** | what `scan` and `revoke` call it |
| **Kind** | shown after the name, with the reference, when there is one: `TLS`, `Code signing`, `Root` |
| **Reference** | anything that says which one it is |
| **Serial** | its serial number in hex, colons or not. Checked against the certificate's PEM, when there is one |
| **SHA-256 fingerprint** | 64 hex digits, upper or lower case, no colons. What the certificate is matched by |

Only the fingerprint, the last field, is needed. With fewer fields, they're taken from the left: a name and a
fingerprint; a name, a kind and a fingerprint; and so on. The serial is read only when all five are there. A line with
the fingerprint alone is shown under its fingerprint.

A line that starts with `#` is a comment, and so are empty lines. A fingerprint that's there twice counts once.

### Leaving one alone

Put `#` in front of its line. For example, to keep **emSign ECC Root CA - G3**, which distrusts everything eMudhra
issued under it:

```
# emSign ECC Root CA - G3	Root	trust anchor	3cf607a968700eda8b84	86A1ECBA089C4A8D3BBE2734C612BA341D813E043CF9E8A862CD5C57A36BBE6B
```

Taking a line out doesn't undo a `revoke` that already distrusted it. [`restore`](backup-and-restore.md) undoes the
whole `revoke`; run `revoke` again afterwards with the new list.

## certs/

`certs/<SHA-256>.pem` is the certificate of a line: its fingerprint in capitals (Linux tells the difference), then
`.pem`. The first certificate
in the file is the one read.

| | |
|---|---|
| With a PEM | the certificate is distrusted even on a computer that doesn't have it yet: put into **Disallowed** on Windows, into the blocklists on Linux |
| Without one | it's only taken out of the stores it's found in. `scan` and `revoke` say so, as a `note:` |

Each PEM is checked against its line: its fingerprint has to be the line's, or the program stops with
`certs/<SHA-256>.pem is not that certificate: its fingerprint is <SHA-256>.`. The serial on the line has to be the
end of the PEM's serial, leading zeros aside; when it isn't, there's a `note:`, and the program goes on.

## Adding a certificate

1. Save the certificate as PEM.
2. Find its SHA-256 fingerprint. With OpenSSL, and the colons taken out:

   ```
   openssl x509 -in cert.pem -noout -fingerprint -sha256
   ```

   or in PowerShell 7:

   ```
   [Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPem((Get-Content -Raw cert.pem)).GetCertHashString('SHA256')
   ```

3. Rename the file `<SHA-256>.pem`, in capitals, and put it in `certs/`.
4. Add its line to `badcerts.txt`.
5. Run `emudhra-patch scan` to see it read, and where it is.

## Your own list, somewhere else

```
./emudhra-patch scan --list /etc/emudhra/mine.txt
sudo ./emudhra-patch revoke --list /etc/emudhra/mine.txt
```

reads that list, and the PEMs in `certs/` beside it (`/etc/emudhra/certs/`).

Without `--list`, the program reads `badcerts.txt` beside itself, or, when there's none there, in the current
directory. When it finds neither, it stops: `badcerts.txt was not found beside the program or in the current
directory; say where it is with --list.`

## On an update

[`update`](installing-and-updating.md#update) replaces `badcerts.txt` beside the program with the new release's, and
adds the new release's PEMs to `certs/`. When the list beside the program isn't the same as the new one, the one that
was there is kept as `badcerts.txt.old`, so your changes aren't lost, but they're no longer in use. A list somewhere
else, read with `--list`, isn't touched.

← [What it doesn't touch](what-it-doesnt-touch.md) · [Contents](README.md) · [Messages and exit codes](messages.md) →

<!-- strongthium-links -->

---

<p align="center"><sub>
<b>Strongthium open-source tools:</b> <b>eMudhra Security Patch</b> · <a href="https://github.com/strongthium/folder-size">Folder Size</a> · <a href="https://github.com/strongthium/health-reminder">Health Reminder</a> · <a href="https://github.com/strongthium/host-dossier">Host Dossier</a> · <a href="https://github.com/strongthium/outbound-watch">Outbound Watch</a> · <a href="https://github.com/strongthium/raw-convert">RAW Convert</a> · <a href="https://github.com/strongthium/video-dispatch">Video Dispatch</a> · <a href="https://github.com/strongthium/webdav-server">WebDAV Server</a><br>
<b>More from Strongthium:</b> <a href="https://strongthium.com/linux-server-firewall">Guard</a> · <a href="https://strongthium.com/self-hosted-mail-server">Mail</a> · <a href="https://strongthium.com/self-hosted-live-chat">Chat</a> · <a href="https://strongthium.com/windows-ssh-client">Remote</a> · <a href="https://strongthium.com/twilio-sms-desktop-app">SMS</a> · <a href="https://strongthium.com/time-tracking-invoicing-software">Work</a> · <a href="https://strongthium.com/windows-registry-editor">Registry</a> · <a href="https://strongthium.com/cfml-server">DotCF</a> · <a href="https://strongthium.com/video-conferencing-dial-up-modem">Convey</a><br>
<a href="https://strongthium.com">Strongthium Labs</a> · Take back control of your internet.
</sub></p>
