using System.Runtime.InteropServices;

namespace Strongthium.EmudhraPatch;

/// <summary><c>about</c> and <c>update</c>: Strongthium, this program, and whether there's a newer version of it.</summary>
internal static class AboutCommand
{
    private const string Text = """
        Strongthium Labs makes small, focused software that does one job well, for the people who keep
        machines running. Everyone else is centralizing and harvesting data. We make software that runs on
        your own machines instead: your data and your keys never pass through us, and everything it does is
        shown, with the reason why.

        More from Strongthium, each with a free edition and no time limit:

          Strongthium Guard    a firewall for Linux servers     https://strongthium.com/linux-server-firewall
          Strongthium Mail     a mail server of your own        https://strongthium.com/self-hosted-mail-server
          Strongthium Chat     live chat you host yourself      https://strongthium.com/self-hosted-live-chat
          Strongthium Remote   an SSH client for Windows        https://strongthium.com/windows-ssh-client

        Website   https://strongthium.com
        Our tools https://github.com/strongthium
        Security  https://github.com/strongthium/.github/blob/main/SECURITY.md
        """;

    private static Updater NewUpdater()
    {
        var platform = (OperatingSystem.IsWindows() ? "win-" : "linux-") + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        return new Updater(Updater.NewClient("emudhra-patch"), "strongthium/emudhra-security-patch",
            version => $"emudhra-patch-{version}-{platform}{(OperatingSystem.IsWindows() ? ".zip" : ".tar.gz")}",
            OperatingSystem.IsWindows() ? "emudhra-patch.exe" : "emudhra-patch");
    }

    public static int About(bool yes)
    {
        Console.WriteLine($"Strongthium eMudhra Security Patch {Updater.RunningVersion.ToString(3)}");
        Console.WriteLine("Free and open source, under the MIT license: https://github.com/strongthium/emudhra-security-patch");
        Console.WriteLine();
        Console.WriteLine(Text);
        Console.WriteLine();
        return Check(install: false, yes);
    }

    public static int Update(bool yes) => Check(install: true, yes);

    /// <summary>Says whether there's a newer version; offers to install it when someone is there to answer.</summary>
    private static int Check(bool install, bool yes)
    {
        var updater = NewUpdater();
        var running = Environment.ProcessPath ?? "";
        try
        {
            Console.WriteLine("Asking GitHub for the latest version...");
            var release = updater.LatestAsync().GetAwaiter().GetResult();
            var latest = release.Version.ToString(3);
            if (release.Version <= Updater.RunningVersion)
            {
                Console.WriteLine(release.Version == Updater.RunningVersion ? "You have the latest version." : $"This is newer than the latest release, {latest}.");
                return 0;
            }

            Console.WriteLine($"Version {latest} is out. You have {Updater.RunningVersion.ToString(3)}. What's new: {release.Page}");
            if (!yes)
            {
                if (Console.IsInputRedirected)
                {
                    if (install) Console.Error.WriteLine("error: update asks before it changes anything; pass --yes when nobody is there to answer.");
                    return install ? 1 : 0;
                }
                Console.Write($"Update to {latest} now? It's checked against the release's checksums first. [y/N] ");
                if (!(Console.ReadLine() ?? "").Trim().StartsWith('y')) return 0;
            }
            updater.InstallAsync(release, running).GetAwaiter().GetResult();
            Console.WriteLine($"Updated to {latest}. Certificates it distrusted stay distrusted; run \"emudhra-patch scan\" to see the new list against this computer.");
            return 0;
        }
        catch (Exception exception) when (Updater.IsExpected(exception))
        {
            Console.Error.WriteLine("error: " + Updater.Explain(exception, running));
            Console.Error.WriteLine($"The releases are at {updater.ReleasesPage}");
            return 1;
        }
    }
}
