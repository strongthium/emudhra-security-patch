using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Strongthium.EmudhraPatch;

/// <summary>
/// Windows' certificate stores. A certificate is taken out of every store that trusts it (Root, AuthRoot - the
/// roots Windows Update brings - CA, TrustedPublisher, TrustedPeople, My) and put into Disallowed, which Windows
/// checks before any of them: a root it would fetch again later is refused too.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsTrust(bool currentUserOnly) : ITrustStore
{
    /// <summary>
    /// AuthRoot before Root: the machine's Root store shows AuthRoot's certificates as well, and a certificate is
    /// taken out of (and put back into) the store it is really in. Once it is out of AuthRoot, Root no longer shows it.
    /// </summary>
    private static readonly string[] Trusting = ["AuthRoot", "Root", "CA", "TrustedPublisher", "TrustedPeople", "My"];
    private const string Disallowed = "Disallowed";

    /// <summary>The machine's stores before the user's: the user's Root store shows the machine's roots as well, and those go out of the machine's.</summary>
    private StoreLocation[] Locations => currentUserOnly ? [StoreLocation.CurrentUser] : [StoreLocation.LocalMachine, StoreLocation.CurrentUser];

    /// <summary>Where the certificates are distrusted: the machine's Disallowed store, or the user's with --current-user.</summary>
    private StoreLocation DistrustIn => currentUserOnly ? StoreLocation.CurrentUser : StoreLocation.LocalMachine;

    public string BackupDirectory => currentUserOnly
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Strongthium", "emudhra-security-patch")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Strongthium", "emudhra-security-patch");

    public List<Finding> Scan()
    {
        var found = new List<Finding>();
        foreach (var location in Locations)
        foreach (var name in Trusting.Append(Disallowed))
        {
            using var store = new X509Store(name, location);
            try { store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly); }
            catch (CryptographicException) { continue; }
            foreach (var certificate in store.Certificates)
                found.Add(new Finding(certificate, BadCerts.Fingerprint(certificate), $@"{location}\{name}", name == Disallowed, (location, name)));
        }
        return found;
    }

    /// <summary>Disallowed wins over every other store.</summary>
    public bool Trusts(IReadOnlyList<Finding> copies) => copies.Any(copy => !copy.Distrusted) && !copies.Any(copy => copy.Distrusted);

    public void Revoke(IReadOnlyList<Target> targets, IReadOnlyList<Finding> found, Manifest manifest, Action<string> say)
    {
        foreach (var finding in found.Where(finding => !finding.Distrusted))
        {
            var (location, name) = ((StoreLocation, string))finding.Source;
            using var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
            // Gone already when it was the machine's certificate seen through the user's store.
            var present = Find(store, finding.Sha256);
            if (present is null) continue;

            var backup = manifest.BackupFile($"{location}-{name}-{finding.Sha256}.cer");
            File.WriteAllBytes(backup, present.RawData);
            manifest.Add("win-removed", location.ToString(), name, finding.Sha256, backup);
            store.Remove(present);
            say($"removed from {location}\\{name}: {present.GetNameInfo(X509NameType.SimpleName, false)}");
        }

        using var disallowed = new X509Store(Disallowed, DistrustIn);
        disallowed.Open(OpenFlags.ReadWrite);
        foreach (var target in targets)
        {
            var certificate = target.Certificate ?? found.FirstOrDefault(finding => finding.Sha256 == target.Sha256)?.Certificate;
            if (certificate is null || Find(disallowed, target.Sha256) is not null) continue;
            manifest.Add("win-disallowed", DistrustIn.ToString(), Disallowed, target.Sha256);
            disallowed.Add(certificate);
            say($"distrusted in {DistrustIn}\\{Disallowed}: {target.Name}");
        }
    }

    public void Restore(Manifest manifest, Action<string> say)
    {
        foreach (var change in manifest.ReadBackwards())
        {
            switch (change)
            {
                case ["win-disallowed", var location, var name, var sha256]:
                {
                    using var store = new X509Store(name, Enum.Parse<StoreLocation>(location));
                    store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                    if (Find(store, sha256) is { } certificate)
                    {
                        store.Remove(certificate);
                        say($"no longer distrusted in {location}\\{name}: {certificate.GetNameInfo(X509NameType.SimpleName, false)}");
                    }
                    break;
                }
                case ["win-removed", var location, var name, var sha256, var backup]:
                {
                    using var store = new X509Store(name, Enum.Parse<StoreLocation>(location));
                    store.Open(OpenFlags.ReadWrite);
                    if (Find(store, sha256) is null)
                    {
                        var certificate = X509CertificateLoader.LoadCertificateFromFile(backup);
                        store.Add(certificate);
                        say($"put back into {location}\\{name}: {certificate.GetNameInfo(X509NameType.SimpleName, false)}");
                    }
                    break;
                }
            }
        }
    }

    private static X509Certificate2? Find(X509Store store, string sha256) =>
        store.Certificates.FirstOrDefault(certificate => BadCerts.Fingerprint(certificate) == sha256);
}
