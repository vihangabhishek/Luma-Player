using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LumaPlayer;

public sealed record PlayerUpdate(Version Version, string Title, Uri DownloadUrl, long ExpectedSize, string Sha256);

internal static class PlayerUpdater
{
    private const string Repo = "vihangaabishek5-lang/Luma-Player";
    private static readonly Uri Api = new($"https://api.github.com/repos/{Repo}/releases?per_page=15");
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LumaPlayer", "1.1"));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return http;
    }

    internal static async Task<PlayerUpdate?> CheckAsync(CancellationToken cancel = default)
    {
        using var response = await Http.GetAsync(Api, HttpCompletionOption.ResponseHeadersRead, cancel);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancel);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancel);
        Version installed = typeof(PlayerUpdater).Assembly.GetName().Version ?? new Version(1, 1, 0);
        PlayerUpdate? latest = null;

        foreach (var release in doc.RootElement.EnumerateArray())
        {
            if (release.GetProperty("draft").GetBoolean()) continue;
            string tag = release.GetProperty("tag_name").GetString() ?? "";
            var match = Regex.Match(tag, @"^v(\d+\.\d+\.\d+)(?:-preview)?$", RegexOptions.IgnoreCase);
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version)) continue;
            if (version <= installed || (latest != null && latest.Version >= version)) continue;

            string fileName = "Luma-Player-Setup-" + version + ".exe";
            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                if (!String.Equals(asset.GetProperty("name").GetString(), fileName, StringComparison.OrdinalIgnoreCase))
                    continue;
                var rawUrl = asset.GetProperty("browser_download_url").GetString();
                var digest = asset.TryGetProperty("digest", out var hash) ? hash.GetString() ?? "" : "";
                var bytes = asset.GetProperty("size").GetInt64();
                if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri) ||
                    uri.Scheme != Uri.UriSchemeHttps || uri.Host != "github.com" ||
                    !uri.AbsolutePath.StartsWith("/vihangaabishek5-lang/Luma-Player/releases/download/", StringComparison.Ordinal) ||
                    !uri.AbsolutePath.EndsWith("/" + fileName, StringComparison.OrdinalIgnoreCase) ||
                    !Regex.IsMatch(digest, @"^sha256:[a-f0-9]{64}$", RegexOptions.IgnoreCase) ||
                    bytes < 1_000_000 || bytes > 900_000_000)
                    continue; // Never install unchecked or non-official executables.
                latest = new PlayerUpdate(version,
                    release.GetProperty("name").GetString() ?? "Luma Player Update",
                    uri, bytes, digest["sha256:".Length..].ToLowerInvariant());
            }
        }
        return latest;
    }

    internal static async Task<string> DownloadVerifiedAsync(PlayerUpdate update, IProgress<double> progress, CancellationToken cancel = default)
    {
        string folder = Path.Combine(Path.GetTempPath(), "LumaPlayer", "updates");
        Directory.CreateDirectory(folder);
        string destination = Path.Combine(folder, "Luma-Player-Setup-" + update.Version + ".exe");
        string temporary = destination + ".download";
        if (File.Exists(temporary)) File.Delete(temporary);
        try
        {
            using var response = await Http.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancel);
            response.EnsureSuccessStatusCode();
            // GitHub asset download redirects to GitHub's secure release assets CDN.
            if (response.Content.Headers.ContentLength is long length && length != update.ExpectedSize)
                throw new InvalidDataException("Update download size differs from its published release.");
            await using (var input = await response.Content.ReadAsStreamAsync(cancel))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true))
            {
                var chunk = new byte[1024 * 128];
                long total = 0;
                while (true)
                {
                    int received = await input.ReadAsync(chunk, cancel);
                    if (received == 0) break;
                    total += received;
                    if (total > update.ExpectedSize) throw new InvalidDataException("Update exceeds expected size.");
                    await output.WriteAsync(chunk.AsMemory(0, received), cancel);
                    progress.Report(Math.Min(99.0, 100.0 * total / update.ExpectedSize));
                }
                if (total != update.ExpectedSize) throw new InvalidDataException("Incomplete update download.");
            }
            await using (var file = File.OpenRead(temporary))
            {
                string sha = Convert.ToHexString(await SHA256.HashDataAsync(file, cancel)).ToLowerInvariant();
                if (!String.Equals(sha, update.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Update SHA-256 checksum does not match the official GitHub release.");
            }
            File.Move(temporary, destination, true);
            progress.Report(100);
            return destination;
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch { }
            throw;
        }
    }

    internal static void LaunchInstaller(string verifiedInstallerPath)
    {
        var filename = Path.GetFileName(verifiedInstallerPath);
        if (!Regex.IsMatch(filename, @"^Luma-Player-Setup-\d+\.\d+\.\d+\.exe$") || !File.Exists(verifiedInstallerPath))
            throw new InvalidOperationException("No verified Luma Player installer was downloaded.");
        // Standard interactive Inno Setup wizard, never silent / bypassing Windows protection.
        Process.Start(new ProcessStartInfo(verifiedInstallerPath)
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(verifiedInstallerPath)!
        }) ?? throw new InvalidOperationException("Windows could not launch the installer.");
    }
}
