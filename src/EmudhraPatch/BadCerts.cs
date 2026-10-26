using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Strongthium.EmudhraPatch;

/// <summary>
/// A certificate to distrust: a line of badcerts.txt, or one found on the computer that belongs with one (the same
/// key, or the issuer of one). The certificate itself is there when certs\ has its PEM or the computer has it.
/// </summary>
internal sealed record Target(string Name, string Kind, string Reference, string Sha256, string? Serial, X509Certificate2? Certificate, string Why);

/// <summary>badcerts.txt, read: its lines, and the PEMs of certs\ beside it.</summary>
internal static partial class BadCerts
{
    [GeneratedRegex("^[0-9A-Fa-f]{64}$")]
    private static partial Regex Sha256Pattern();

    [GeneratedRegex("\t| {2,}")]
    private static partial Regex FieldSeparator();

    /// <summary>The list's certificates; a line that cannot be read, or a PEM that is not the certificate its line says, is an exception that says which.</summary>
    public static List<Target> Load(string listPath, string certsDirectory, List<string> warnings)
    {
        var targets = new List<Target>();
        var number = 0;
        foreach (var raw in File.ReadLines(listPath))
        {
            number++;
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var fields = FieldSeparator().Split(line).Select(field => field.Trim()).Where(field => field.Length > 0).ToArray();
            var sha256 = fields.Length > 0 ? fields[^1] : "";
            if (!Sha256Pattern().IsMatch(sha256))
                throw new InvalidDataException($"{listPath}, line {number}: the last field is not a SHA-256 fingerprint (64 hex digits).");
            sha256 = sha256.ToUpperInvariant();

            var name = fields.Length > 1 ? fields[0] : sha256;
            var kind = fields.Length > 2 ? fields[1] : "";
            var reference = fields.Length > 3 ? fields[2] : "";
            var serial = fields.Length >= 5 ? fields[3].Replace(":", "").ToUpperInvariant() : null;

            var certificate = ReadPem(Path.Combine(certsDirectory, sha256 + ".pem"));
            if (certificate is not null)
            {
                var actual = Fingerprint(certificate);
                if (actual != sha256)
                    throw new InvalidDataException($"certs{Path.DirectorySeparatorChar}{sha256}.pem is not that certificate: its fingerprint is {actual}.");
                if (serial is not null && !certificate.SerialNumber.TrimStart('0').EndsWith(serial.TrimStart('0'), StringComparison.Ordinal))
                    warnings.Add($"{name}: badcerts.txt says serial {serial}, the certificate has {certificate.SerialNumber}.");
            }
            else
            {
                warnings.Add($"{name}: no certs{Path.DirectorySeparatorChar}{sha256}.pem, so it is only removed where it is found, not distrusted ahead of time.");
            }

            if (targets.Any(target => target.Sha256 == sha256)) continue;
            targets.Add(new Target(name, kind, reference, sha256, serial, certificate, "badcerts.txt"));
        }
        return targets;
    }

    /// <summary>The first certificate of a PEM file, or null when there is no such file.</summary>
    private static X509Certificate2? ReadPem(string path)
    {
        if (!File.Exists(path)) return null;
        return X509Certificate2.CreateFromPem(File.ReadAllText(path));
    }

    public static string Fingerprint(X509Certificate2 certificate) => certificate.GetCertHashString(HashAlgorithmName.SHA256).ToUpperInvariant();

    /// <summary>The certificate as PEM text, for a blocklist file or a backup.</summary>
    public static string Pem(X509Certificate2 certificate) => certificate.ExportCertificatePem() + "\n";

    /// <summary>The certificate's key as DER SubjectPublicKeyInfo, hex: two certificates with the same one are the same CA.</summary>
    public static string KeyOf(X509Certificate2 certificate) => Convert.ToHexString(certificate.PublicKey.ExportSubjectPublicKeyInfo());

    /// <summary>Whether <paramref name="issuer"/> could have signed <paramref name="child"/>: its subject is the child's issuer, and their key identifiers agree where both have one.</summary>
    public static bool Issued(X509Certificate2 issuer, X509Certificate2 child)
    {
        if (!issuer.SubjectName.RawData.AsSpan().SequenceEqual(child.IssuerName.RawData)) return false;
        if (issuer.RawData.AsSpan().SequenceEqual(child.RawData)) return false;
        var authorityKey = AuthorityKeyId(child);
        var subjectKey = SubjectKeyId(issuer);
        return authorityKey is null || subjectKey is null || authorityKey == subjectKey;
    }

    private static string? SubjectKeyId(X509Certificate2 certificate)
    {
        var extension = certificate.Extensions["2.5.29.14"];
        if (extension is null) return null;
        var ski = extension as X509SubjectKeyIdentifierExtension ?? new X509SubjectKeyIdentifierExtension(extension, extension.Critical);
        return ski.SubjectKeyIdentifier?.ToUpperInvariant();
    }

    private static string? AuthorityKeyId(X509Certificate2 certificate)
    {
        var extension = certificate.Extensions["2.5.29.35"];
        if (extension is null) return null;
        var aki = extension as X509AuthorityKeyIdentifierExtension ?? new X509AuthorityKeyIdentifierExtension(extension.RawData, extension.Critical);
        return aki.KeyIdentifier is { } key ? Convert.ToHexString(key.Span) : null;
    }
}
