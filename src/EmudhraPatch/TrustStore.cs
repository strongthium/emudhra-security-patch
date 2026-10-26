using System.Security.Cryptography.X509Certificates;

namespace Strongthium.EmudhraPatch;

/// <summary>
/// A certificate where the computer keeps one: <see cref="Where"/> says where for people, <see cref="Source"/> is
/// what the store needs to take it out again. <see cref="Distrusted"/> is a place that distrusts what is in it
/// (Windows' Disallowed store, a Linux blocklist) rather than trusting it.
/// </summary>
internal sealed record Finding(X509Certificate2 Certificate, string Sha256, string Where, bool Distrusted, object Source);

/// <summary>The computer's trust stores, as one platform keeps them.</summary>
internal interface ITrustStore
{
    /// <summary>Where the changes are written down for restore, and the removed certificates kept.</summary>
    string BackupDirectory { get; }

    /// <summary>Every certificate the trust stores hold, distrusted ones included.</summary>
    List<Finding> Scan();

    /// <summary>Whether the computer trusts a certificate, from every place it was found in (none: it does not).</summary>
    bool Trusts(IReadOnlyList<Finding> copies);

    /// <summary>
    /// Takes the targets that were found out of the places that trust them, and distrusts every target whose
    /// certificate is known, found or not. Each change goes into <paramref name="manifest"/> before it is made.
    /// </summary>
    void Revoke(IReadOnlyList<Target> targets, IReadOnlyList<Finding> found, Manifest manifest, Action<string> say);

    /// <summary>Undoes what the manifest says was done, the last change first.</summary>
    void Restore(Manifest manifest, Action<string> say);
}

/// <summary>
/// manifest.txt in the backup directory: one change a line, its fields separated by tabs, the kind first. Restore
/// reads it backwards and then moves it aside, so that a second revoke starts a manifest of its own.
/// </summary>
internal sealed class Manifest(string directory)
{
    public string Directory { get; } = directory;
    public string Path => System.IO.Path.Combine(Directory, "manifest.txt");

    public void Add(params string[] fields)
    {
        System.IO.Directory.CreateDirectory(Directory);
        File.AppendAllText(Path, string.Join('\t', fields) + "\n");
    }

    /// <summary>The changes, the last one first; none when nothing was revoked.</summary>
    public List<string[]> ReadBackwards()
    {
        if (!File.Exists(Path)) return [];
        var lines = File.ReadAllLines(Path).Where(line => line.Length > 0).Select(line => line.Split('\t')).ToList();
        lines.Reverse();
        return lines;
    }

    /// <summary>A file in the backup directory for a copy of something that is about to change.</summary>
    public string BackupFile(string name)
    {
        System.IO.Directory.CreateDirectory(Directory);
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        return System.IO.Path.Combine(Directory, stamp + "-" + name);
    }

    public void Close()
    {
        if (!File.Exists(Path)) return;
        File.Move(Path, System.IO.Path.Combine(Directory, $"manifest.restored-{DateTime.UtcNow:yyyyMMddHHmmss}.txt"));
    }
}
