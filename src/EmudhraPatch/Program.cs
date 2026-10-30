using System.Security.Cryptography.X509Certificates;

namespace Strongthium.EmudhraPatch;

internal static class Program
{
    private const string Usage = """
        emudhra-patch: finds the certificates of badcerts.txt in this computer's trust stores and distrusts them,
        and the roots that issued them.

          emudhra-patch [scan]      says where each certificate is, and changes nothing
          emudhra-patch revoke      takes them out of the stores that trust them and distrusts them (administrator / root)
          emudhra-patch restore     undoes what revoke did, from the backup it made
          emudhra-patch about       who makes this, and whether there's a newer version (GitHub is asked)
          emudhra-patch update      installs the newer version and its list, if there is one: it asks first

          --list <file>    the list to use (default: badcerts.txt beside the program, then in the current directory);
                            the PEMs are read from certs/ beside it
          --current-user    Windows: only the current user's stores, which needs no administrator
          --yes             revoke or update without asking first

        Exit codes: 0 nothing on the list is trusted (any more), 1 error, 2 something on the list is still trusted,
        3 not run as administrator / root.
        """;

    private static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or System.Security.Cryptography.CryptographicException)
        {
            Console.Error.WriteLine("error: " + exception.Message);
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        Updater.Tidy();
        string command = "scan", list = "";
        bool currentUser = false, yes = false;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "scan" or "revoke" or "restore" or "about" or "update": command = args[i]; break;
                case "--list" when i + 1 < args.Length: list = args[++i]; break;
                case "--current-user": currentUser = true; break;
                case "--yes" or "-y": yes = true; break;
                case "help" or "--help" or "-h" or "/?": Console.WriteLine(Usage); return 0;
                default: Console.Error.WriteLine($"unknown argument: {args[i]}\n\n{Usage}"); return 1;
            }
        }

        if (command == "about") return AboutCommand.About(yes);
        if (command == "update") return AboutCommand.Update(yes);

        ITrustStore store;
        if (OperatingSystem.IsWindows()) store = new WindowsTrust(currentUser);
        else if (OperatingSystem.IsLinux()) store = new LinuxTrust();
        else { Console.Error.WriteLine("error: this runs on Windows and Linux."); return 1; }

        var manifest = new Manifest(store.BackupDirectory);
        var needsPrivilege = command != "scan" && !(OperatingSystem.IsWindows() && currentUser);
        if (needsPrivilege && !Environment.IsPrivilegedProcess)
        {
            Console.Error.WriteLine(OperatingSystem.IsWindows()
                ? $"error: {command} changes the machine's certificate stores: run it as administrator (or with --current-user for your own stores only)."
                : $"error: {command} changes the system's CA certificates: run it as root (sudo).");
            return 3;
        }

        if (command == "restore")
        {
            if (!File.Exists(manifest.Path))
            {
                Console.WriteLine($"Nothing to restore: there is no {manifest.Path}.");
                return 0;
            }
            store.Restore(manifest, Say);
            manifest.Close();
            Console.WriteLine("Restored.");
            return 0;
        }

        list = list.Length > 0 ? Path.GetFullPath(list) : FindList();
        var warnings = new List<string>();
        var targets = BadCerts.Load(list, Path.Combine(Path.GetDirectoryName(list)!, "certs"), warnings);
        Console.WriteLine($"{targets.Count} certificates in {list}");
        foreach (var warning in warnings) Console.WriteLine("  note: " + warning);

        var found = store.Scan();
        Expand(targets, found);
        var trusted = Report(targets, found, store);

        if (command == "scan")
        {
            Console.WriteLine();
            Console.WriteLine(trusted.Count == 0
                ? "Nothing on the list is trusted on this computer."
                : $"{trusted.Count} trusted. Run \"emudhra-patch revoke\" as {(OperatingSystem.IsWindows() ? "administrator" : "root")} to distrust them; it also distrusts the ones with a PEM that are not here yet.");
            return trusted.Count == 0 ? 0 : 2;
        }

        if (!yes)
        {
            if (Console.IsInputRedirected)
            {
                Console.Error.WriteLine("error: revoke asks before it changes anything; pass --yes when nobody is there to answer.");
                return 1;
            }
            Console.WriteLine();
            Console.Write($"Distrust these {targets.Count} certificates on this computer? A backup goes to {store.BackupDirectory}. [y/N] ");
            if (!(Console.ReadLine() ?? "").Trim().StartsWith('y')) { Console.WriteLine("Nothing was changed."); return 0; }
        }

        Console.WriteLine();
        var listed = targets.Select(target => target.Sha256).ToHashSet();
        store.Revoke(targets, found.Where(finding => listed.Contains(finding.Sha256)).ToList(), manifest, Say);

        Console.WriteLine();
        Console.WriteLine("Checking again:");
        var after = Report(targets, store.Scan(), store);
        Console.WriteLine();
        Console.WriteLine(after.Count == 0
            ? $"Done: nothing on the list is trusted any more. \"emudhra-patch restore\" undoes it."
            : $"{after.Count} still trusted (see above).");
        return after.Count == 0 ? 0 : 2;
    }

    private static void Say(string line) => Console.WriteLine("  " + line);

    private static string FindList()
    {
        foreach (var directory in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            var path = Path.Combine(directory, "badcerts.txt");
            if (File.Exists(path)) return path;
        }
        throw new IOException("badcerts.txt was not found beside the program or in the current directory; say where it is with --list.");
    }

    /// <summary>
    /// Adds what belongs with the targets among the certificates found: one with the key of a target (another
    /// issue or cross-signature of the same CA), and the issuers of a target, until nothing more is added.
    /// </summary>
    private static void Expand(List<Target> targets, List<Finding> found)
    {
        var candidates = found.Select(finding => finding.Certificate)
            .Concat(targets.Select(target => target.Certificate).OfType<X509Certificate2>())
            .ToList();
        bool added;
        do
        {
            added = false;
            var known = targets.Select(target => target.Sha256).ToHashSet();
            var keys = targets.Where(target => target.Certificate is not null)
                .GroupBy(target => BadCerts.KeyOf(target.Certificate!))
                .ToDictionary(group => group.Key, group => group.First());
            foreach (var candidate in candidates)
            {
                var sha256 = BadCerts.Fingerprint(candidate);
                if (known.Contains(sha256)) continue;
                var why = keys.TryGetValue(BadCerts.KeyOf(candidate), out var sameKey)
                    ? $"same key as {sameKey.Name}"
                    : targets.FirstOrDefault(target => target.Certificate is not null && BadCerts.Issued(candidate, target.Certificate)) is { } child
                        ? $"issued {child.Name}"
                        : null;
                if (why is null) continue;
                targets.Add(new Target(candidate.GetNameInfo(X509NameType.SimpleName, false), "found", "", sha256, candidate.SerialNumber, candidate, why));
                known.Add(sha256);
                added = true;
            }
        } while (added);
    }

    /// <summary>Prints where each target is; returns the ones the computer trusts.</summary>
    private static List<Target> Report(List<Target> targets, List<Finding> found, ITrustStore store)
    {
        var trusted = new List<Target>();
        foreach (var target in targets)
        {
            var here = found.Where(finding => finding.Sha256 == target.Sha256).ToList();
            var trusts = store.Trusts(here);
            if (trusts) trusted.Add(target);

            var state = trusts ? "TRUSTED" : here.Count > 0 ? "distrusted" : "not here";
            var origin = target.Why != "badcerts.txt" ? $" ({target.Why})" : target.Reference.Length > 0 ? $" ({target.Kind}, {target.Reference})" : "";
            Console.WriteLine();
            Console.WriteLine($"[{state,-10}] {target.Name}{origin}");
            Console.WriteLine($"             SHA-256 {target.Sha256}");
            foreach (var finding in here) Console.WriteLine($"             {(finding.Distrusted ? "distrusted in" : "in")} {finding.Where}");
        }
        return trusted;
    }
}
