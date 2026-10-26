using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Strongthium.EmudhraPatch;

/// <summary>
/// The CA certificates of a Linux distribution. They come from a few places that are sources - the distribution's
/// package, the machine's own anchors - and are built from those into the bundles programs read (/etc/ssl/certs,
/// /etc/pki/ca-trust/extracted, ...). So the sources are changed and the bundles built again:
/// <list type="bullet">
/// <item>Debian and Ubuntu: a packaged root is deselected in /etc/ca-certificates.conf (a "!" in front of it), then update-ca-certificates.</item>
/// <item>Fedora, RHEL, Arch, openSUSE (p11-kit): every target goes into the blocklist directory, which wins over every anchor, then update-ca-trust.</item>
/// <item>Everywhere: a target among the machine's own anchors (/usr/local/share/ca-certificates, .../source/anchors) is taken out of its file.</item>
/// </list>
/// </summary>
internal sealed class LinuxTrust : ITrustStore
{
    private enum Kind
    {
        /// <summary>The distribution's, listed in /etc/ca-certificates.conf.</summary>
        DebianPackaged,
        /// <summary>The distribution's, in a p11-kit source directory: only a blocklist overrules it.</summary>
        Packaged,
        /// <summary>The machine's own: a file that may be edited.</summary>
        Anchor,
        /// <summary>Built from the sources: right again once they are.</summary>
        Generated,
        /// <summary>A blocklist: distrusted.</summary>
        Blocked,
    }

    private sealed record Place(string Path, Kind Kind);

    private const string DebianConf = "/etc/ca-certificates.conf";
    private const string DebianPackageDirectory = "/usr/share/ca-certificates/";

    private static readonly Place[] Places =
    [
        new("/usr/local/share/ca-certificates", Kind.Anchor),
        new("/etc/pki/ca-trust/source", Kind.Anchor),
        new("/etc/ca-certificates/trust-source", Kind.Anchor),
        new("/etc/pki/trust", Kind.Anchor),
        new("/usr/share/ca-certificates", Kind.Packaged),
        new("/usr/share/pki/ca-trust-source", Kind.Packaged),
        new("/usr/share/pki/trust", Kind.Packaged),
        new("/etc/ssl/certs", Kind.Generated),
        new("/etc/ssl/ca-bundle.pem", Kind.Generated),
        new("/etc/pki/ca-trust/extracted", Kind.Generated),
        new("/etc/pki/tls/certs", Kind.Generated),
        new("/etc/ca-certificates/extracted", Kind.Generated),
        new("/var/lib/ca-certificates", Kind.Generated),
    ];

    /// <summary>The p11-kit source directories a blocklist can go into; the folder is called blacklist on older systems.</summary>
    private static readonly string[] BlocklistParents = ["/etc/pki/ca-trust/source", "/etc/ca-certificates/trust-source", "/etc/pki/trust"];

    private static readonly string[] Extensions = [".pem", ".crt", ".cer", ".der", ".p11-kit", ".trust", ""];

    public string BackupDirectory => "/var/lib/emudhra-security-patch";

    public List<Finding> Scan()
    {
        var found = new List<Finding>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var place in Places)
        foreach (var file in Files(place.Path))
        {
            // The links in /etc/ssl/certs point at the packaged files; those are read where they are.
            if (new FileInfo(file).LinkTarget is not null || !seen.Add(file)) continue;
            var kind = KindOf(file, place.Kind);
            foreach (var (certificate, _) in Read(file))
                found.Add(new Finding(certificate, BadCerts.Fingerprint(certificate), file, kind == Kind.Blocked, new Place(file, kind)));
        }
        return found;
    }

    /// <summary>
    /// Trusted when a bundle programs read has it, or when a source has it that the next rebuild would put there:
    /// the machine's own anchors and the distribution's files, unless a blocklist overrules them.
    /// </summary>
    public bool Trusts(IReadOnlyList<Finding> copies)
    {
        var kinds = copies.Select(copy => ((Place)copy.Source).Kind).ToHashSet();
        return kinds.Contains(Kind.Generated)
            || (kinds.Overlaps([Kind.Anchor, Kind.DebianPackaged, Kind.Packaged]) && !kinds.Contains(Kind.Blocked));
    }

    public void Revoke(IReadOnlyList<Target> targets, IReadOnlyList<Finding> found, Manifest manifest, Action<string> say)
    {
        var bad = targets.Select(target => target.Sha256).ToHashSet();

        // The machine's own anchors: out of their files.
        foreach (var group in found.Where(finding => ((Place)finding.Source).Kind == Kind.Anchor).GroupBy(finding => finding.Where))
        {
            if (group.Key.EndsWith(".p11-kit", StringComparison.Ordinal))
            {
                say($"left in {group.Key} (a p11-kit file; the blocklist overrules it)");
                continue;
            }
            var backup = manifest.BackupFile(Path.GetFileName(group.Key));
            File.Copy(group.Key, backup);
            manifest.Add("file", group.Key, backup);
            var kept = Rewrite(group.Key, bad);
            say(kept ? $"removed from {group.Key}" : $"deleted {group.Key} (nothing else was in it)");
        }

        // Debian's packaged roots: deselected.
        var deselect = found
            .Where(finding => ((Place)finding.Source).Kind == Kind.DebianPackaged)
            .Select(finding => finding.Where[DebianPackageDirectory.Length..])
            .ToHashSet(StringComparer.Ordinal);
        if (deselect.Count > 0)
        {
            var lines = File.ReadAllLines(DebianConf);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!deselect.Contains(lines[i].Trim())) continue;
                manifest.Add("deb-deselected", lines[i].Trim());
                say($"deselected in {DebianConf}: {lines[i].Trim()}");
                lines[i] = "!" + lines[i].Trim();
            }
            File.WriteAllLines(DebianConf, lines);
        }

        // p11-kit: every target into the blocklist, whether this machine has it yet or not.
        var blocklists = BlocklistParents.Where(Directory.Exists).Select(BlocklistOf).ToList();
        foreach (var directory in blocklists)
        {
            Directory.CreateDirectory(directory);
            foreach (var target in targets)
            {
                var certificate = target.Certificate ?? found.FirstOrDefault(finding => finding.Sha256 == target.Sha256)?.Certificate;
                if (certificate is null) continue;
                var file = Path.Combine(directory, $"emudhra-security-patch-{target.Sha256[..16].ToLowerInvariant()}.pem");
                if (File.Exists(file)) continue;
                manifest.Add("blocked", file);
                File.WriteAllText(file, BadCerts.Pem(certificate));
                say($"distrusted in {directory}: {target.Name}");
            }
        }

        var packaged = found.Where(finding => ((Place)finding.Source).Kind == Kind.Packaged).ToList();
        if (packaged.Count > 0 && blocklists.Count == 0)
            foreach (var finding in packaged)
                say($"WARNING: {finding.Where} is the distribution's and this system has no p11-kit blocklist to overrule it");

        RebuildBundles(say);
    }

    public void Restore(Manifest manifest, Action<string> say)
    {
        foreach (var change in manifest.ReadBackwards())
        {
            switch (change)
            {
                case ["blocked", var file]:
                    if (File.Exists(file)) File.Delete(file);
                    say($"no longer distrusted: deleted {file}");
                    break;
                case ["deb-deselected", var entry]:
                    var lines = File.ReadAllLines(DebianConf);
                    for (var i = 0; i < lines.Length; i++)
                        if (lines[i].Trim() == "!" + entry) lines[i] = entry;
                    File.WriteAllLines(DebianConf, lines);
                    say($"selected again in {DebianConf}: {entry}");
                    break;
                case ["file", var original, var backup]:
                    Directory.CreateDirectory(Path.GetDirectoryName(original)!);
                    File.Copy(backup, original, overwrite: true);
                    say($"put back: {original}");
                    break;
            }
        }
        RebuildBundles(say);
    }

    /// <summary>Builds the bundles again from their sources, with whichever of the two tools this distribution has.</summary>
    private static void RebuildBundles(Action<string> say)
    {
        var ran = false;
        foreach (var (tool, arguments) in new[] { ("update-ca-certificates", Array.Empty<string>()), ("update-ca-trust", new[] { "extract" }) })
        {
            var path = Which(tool);
            if (path is null) continue;
            ran = true;
            var start = new ProcessStartInfo(path) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            using var process = Process.Start(start)!;
            var error = process.StandardError.ReadToEndAsync();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            foreach (var line in (output + error.Result).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                say($"  {tool}: {line}");
            say(process.ExitCode == 0 ? $"rebuilt the bundles: {tool}" : $"WARNING: {tool} failed with exit code {process.ExitCode}");
        }
        if (!ran) say("WARNING: neither update-ca-certificates nor update-ca-trust is installed: the bundles were not rebuilt");
    }

    private static string? Which(string tool)
    {
        var path = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(':', StringSplitOptions.RemoveEmptyEntries);
        return path.Concat(["/usr/sbin", "/usr/bin", "/sbin", "/bin"])
            .Select(directory => Path.Combine(directory, tool))
            .FirstOrDefault(File.Exists);
    }

    private static string BlocklistOf(string parent)
    {
        var blocklist = Path.Combine(parent, "blocklist");
        var blacklist = Path.Combine(parent, "blacklist");
        if (Directory.Exists(blocklist)) return blocklist;
        if (Directory.Exists(blacklist) || parent == "/etc/pki/trust") return blacklist;
        return blocklist;
    }

    private static Kind KindOf(string file, Kind kind)
    {
        var parts = file.Split('/');
        if (parts.Contains("blocklist") || parts.Contains("blacklist")) return Kind.Blocked;
        // Debian builds its bundle from the files /etc/ca-certificates.conf selects; one it does not select (or
        // deselects with "!") is not trusted.
        if (kind == Kind.Packaged && file.StartsWith(DebianPackageDirectory, StringComparison.Ordinal) && File.Exists(DebianConf))
            return File.ReadLines(DebianConf).Any(line => line.Trim() == file[DebianPackageDirectory.Length..]) ? Kind.DebianPackaged : Kind.Blocked;
        return kind;
    }

    private static IEnumerable<string> Files(string path)
    {
        if (File.Exists(path)) return [path];
        if (!Directory.Exists(path)) return [];
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = 0 };
        return Directory.EnumerateFiles(path, "*", options)
            .Where(file => Extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>The certificates of a file - PEM, a bundle of PEMs, a p11-kit file, or one DER certificate - and where each one's PEM is in its text.</summary>
    private static List<(X509Certificate2 Certificate, Range Location)> Read(string file)
    {
        var certificates = new List<(X509Certificate2, Range)>();
        byte[] bytes;
        try
        {
            if (new FileInfo(file).Length > 64 * 1024 * 1024) return certificates;
            bytes = File.ReadAllBytes(file);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { return certificates; }

        var text = Encoding.ASCII.GetString(bytes);
        if (!text.Contains("-----BEGIN"))
        {
            try { certificates.Add((X509CertificateLoader.LoadCertificate(bytes), Range.All)); }
            catch (CryptographicException) { }
            return certificates;
        }

        var offset = 0;
        while (offset < text.Length && PemEncoding.TryFind(text.AsSpan(offset), out var fields))
        {
            var location = new Range(offset + fields.Location.Start.Value, offset + fields.Location.End.Value);
            // Not "TRUSTED CERTIFICATE": OpenSSL's bundle with trust settings lists the rejected ones that way too.
            if (text.AsSpan(offset)[fields.Label].SequenceEqual("CERTIFICATE"))
            {
                try
                {
                    var der = Convert.FromBase64String(text[(offset + fields.Base64Data.Start.Value)..(offset + fields.Base64Data.End.Value)]);
                    certificates.Add((X509CertificateLoader.LoadCertificate(der), location));
                }
                catch (Exception exception) when (exception is CryptographicException or FormatException) { }
            }
            offset = location.End.Value;
        }
        return certificates;
    }

    /// <summary>Writes the file again without the certificates in <paramref name="bad"/>; deletes it when none are left. True when it is still there.</summary>
    private static bool Rewrite(string file, HashSet<string> bad)
    {
        var certificates = Read(file);
        var keep = certificates.Where(entry => !bad.Contains(BadCerts.Fingerprint(entry.Certificate))).ToList();
        if (keep.Count == 0)
        {
            File.Delete(file);
            return false;
        }

        var text = File.ReadAllText(file);
        var output = new StringBuilder();
        var position = 0;
        foreach (var (certificate, location) in certificates)
        {
            if (!bad.Contains(BadCerts.Fingerprint(certificate))) continue;
            output.Append(text, position, location.Start.Value - position);
            position = location.End.Value;
        }
        output.Append(text, position, text.Length - position);
        File.WriteAllText(file, output.ToString());
        return true;
    }
}
