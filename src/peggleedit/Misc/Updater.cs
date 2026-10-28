// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2026. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
    public enum UpdateRing
    {
        Release,
        Nightly,
    }

    /// <summary>
    /// A published build offered by an update feed (build.json).
    /// </summary>
    public class UpdateInfo
    {
        public string Channel { get; set; }
        public string Version { get; set; }
        public string Sha { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string Url { get; set; }
        public string Sha256 { get; set; }

        public bool IsNightly => Channel == BuildInfo.NightlyChannel;

        /// <summary>
        /// Identifies this build for "skip this version".
        /// </summary>
        public string Key => IsNightly ? $"nightly:{Sha}" : $"release:{Version}";

        public string DisplayVersion => IsNightly ? $"{Version} ({BuildInfo.ShortenCommit(Sha)})" : Version;

        public string PageUrl => IsNightly ?
            $"{Program.AppWebsite}/releases/tag/nightly" :
            $"{Program.AppWebsite}/releases/tag/v{Version}";
    }

    /// <summary>
    /// Checks for, downloads and installs release and nightly updates.
    /// </summary>
    /// <remarks>
    /// Each GitHub release (the rolling "nightly" pre-release and every v* release) carries a build.json
    /// describing peggleedit.zip. The update is downloaded and verified, extracted into a staging folder
    /// next to the executable, and installed after the main window closes: each file being replaced is
    /// renamed aside (Windows allows renaming a running exe or loaded DLL, but not overwriting it), the new
    /// file is moved in, and the new build is started. Any failure part way puts the renamed files back.
    /// The renamed files are listed in a manifest, which the new process deletes once the old process has
    /// exited.
    /// </remarks>
    internal static class Updater
    {
        private const string FeedBaseUrl = Program.AppWebsite + "/releases/";
        private const string AssetKey = "windows";
        private const string StagingDirectoryPrefix = "update-staging-";
        private const string OldFilesManifestName = "update-old-files.txt";
        private const string OldFileSuffix = ".peggleedit-old";

        /// <summary>
        /// Overrides the base URL of the feeds ({base}release/build.json and {base}nightly/build.json),
        /// for testing the updater against a local server.
        /// </summary>
        public const string FeedOverrideVariable = "PEGGLEEDIT_UPDATE_FEED";
        private const string RestartPidVariable = "PEGGLEEDIT_RESTART_PID";
        private const string UpdatedVariable = "PEGGLEEDIT_UPDATED";

        private static readonly HttpClient _httpClient = CreateHttpClient();

        /// <summary>
        /// A verified, extracted update waiting for the main window to close.
        /// </summary>
        public static string PendingInstall { get; set; }

        public static string AppDirectory => Path.GetDirectoryName(Application.ExecutablePath);
        private static string OldFilesManifestPath => Path.Combine(AppDirectory, OldFilesManifestName);
        private static string FeedOverride => Environment.GetEnvironmentVariable(FeedOverrideVariable);

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                // Timeouts are applied per request so that a large download is not cut off.
                Timeout = Timeout.InfiniteTimeSpan
            };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Program.AppTitle, Program.AppVersion));
            return client;
        }

        public static string GetFeedUrl(UpdateRing ring)
        {
            var baseUrl = FeedOverride;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return ring == UpdateRing.Nightly ?
                    FeedBaseUrl + "download/nightly/build.json" :
                    FeedBaseUrl + "latest/download/build.json";
            }
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";
            return baseUrl + (ring == UpdateRing.Nightly ? "nightly" : "release") + "/build.json";
        }

        #region Check

        /// <summary>
        /// Returns the newer build published on <paramref name="ring"/>, or null if there is none.
        /// Throws if the feed can not be fetched or read.
        /// </summary>
        public static async Task<UpdateInfo> CheckAsync(UpdateRing ring, CancellationToken ct)
        {
            if (!BuildInfo.IsPublished)
                return null;

            var url = GetFeedUrl(ring);
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(15));
                using (var response = await _httpClient.GetAsync(url, cts.Token))
                {
                    // Releases published before build.json existed have no feed.
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    if (!response.IsSuccessStatusCode)
                        throw new HttpRequestException($"{url} returned {(int)response.StatusCode} {response.ReasonPhrase}.");

                    var json = await response.Content.ReadAsStringAsync();
                    var info = ParseFeed(json);
                    var newer = IsNewer(
                        Program.CurrentVersion, BuildInfo.Channel, BuildInfo.Commit, BuildInfo.CommitDate,
                        info.Channel, info.Version, info.Sha, info.Date);
                    return newer ? info : null;
                }
            }
        }

        public static UpdateInfo ParseFeed(string json)
        {
            // Ignore a byte order mark
            using (var document = JsonDocument.Parse(json.TrimStart((char)0xFEFF)))
            {
                var root = document.RootElement;
                var info = new UpdateInfo
                {
                    Channel = GetString(root, "channel"),
                    Version = GetString(root, "version"),
                    Sha = GetString(root, "sha"),
                    Date = BuildInfo.ParseDate(GetString(root, "date")),
                };
                if (root.TryGetProperty("assets", out var assets) &&
                    assets.ValueKind == JsonValueKind.Object &&
                    assets.TryGetProperty(AssetKey, out var asset))
                {
                    info.Url = GetString(asset, "url");
                    info.Sha256 = GetString(asset, "sha256");
                }

                if (info.Channel != BuildInfo.ReleaseChannel && info.Channel != BuildInfo.NightlyChannel)
                    throw new InvalidDataException($"The update feed has an unknown channel '{info.Channel}'.");
                if (string.IsNullOrEmpty(info.Version))
                    throw new InvalidDataException("The update feed has no version.");
                if (string.IsNullOrEmpty(info.Url) || string.IsNullOrEmpty(info.Sha256))
                    throw new InvalidDataException("The update feed has no Windows download.");
                if (!info.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(FeedOverride))
                    throw new InvalidDataException("The update feed's download is not HTTPS.");
                return info;
            }
        }

        private static string GetString(JsonElement element, string name)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }

        /// <summary>
        /// Whether a feed build is newer than the running build.
        /// </summary>
        /// <remarks>
        /// A nightly is identified by its commit, and is newer when it is a different commit that was made later.
        /// A release is newer when its version is higher. A nightly's version is the version in development, which
        /// is either the next release (before it is tagged) or the last release (when the version has not been bumped
        /// yet), so a release with the same version as a running nightly is only newer if it was made later.
        /// </remarks>
        public static bool IsNewer(
            Version currentVersion, string currentChannel, string currentCommit, DateTimeOffset? currentDate,
            string channel, string version, string sha, DateTimeOffset? date)
        {
            if (!string.IsNullOrEmpty(sha) && string.Equals(sha, currentCommit, StringComparison.OrdinalIgnoreCase))
                return false;

            if (channel == BuildInfo.NightlyChannel)
            {
                if (string.IsNullOrEmpty(sha))
                    return false;
                if (date != null && currentDate != null)
                    return date > currentDate;
                return true;
            }

            if (channel == BuildInfo.ReleaseChannel)
            {
                if (!TryParseVersion(version, out var available))
                    return false;
                var comparison = available.CompareTo(currentVersion);
                if (comparison != 0)
                    return comparison > 0;
                if (currentChannel == BuildInfo.ReleaseChannel)
                    return false;
                if (date != null && currentDate != null)
                    return date > currentDate;
                // Without dates, treat a release as newer than a pre-release of the same version.
                return true;
            }

            return false;
        }

        private static bool TryParseVersion(string text, out Version version)
        {
            text = (text ?? "").TrimStart('v');
            var dash = text.IndexOf('-');
            if (dash != -1)
                text = text.Substring(0, dash);
            if (Version.TryParse(text, out version))
            {
                version = new Version(version.Major, version.Minor, Math.Max(0, version.Build));
                return true;
            }
            return false;
        }

        #endregion

        #region Download

        /// <summary>
        /// Fails early if the update can not be installed because the application folder is read only
        /// (e.g. PeggleEdit was extracted into Program Files).
        /// </summary>
        public static void EnsureAppDirectoryWritable()
        {
            try
            {
                var path = Path.Combine(AppDirectory, $"update-{Guid.NewGuid():N}.tmp");
                File.WriteAllText(path, "");
                File.Delete(path);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                throw new IOException($"PeggleEdit can not write to its folder ({AppDirectory}). Download the update manually, or move PeggleEdit to a folder you can write to.", ex);
            }
        }

        /// <summary>
        /// Downloads the update to a temporary folder and verifies its checksum. A partial or corrupt
        /// download is deleted. Returns the path of the downloaded zip.
        /// </summary>
        public static async Task<string> DownloadAsync(UpdateInfo info, IProgress<(long done, long total)> progress, CancellationToken ct)
        {
            var directory = Path.Combine(Path.GetTempPath(), $"peggleedit-update-{Guid.NewGuid():N}");
            var path = Path.Combine(directory, "peggleedit.zip");
            Directory.CreateDirectory(directory);
            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(30));
                    using (var response = await _httpClient.GetAsync(info.Url, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                    // Reads on .NET Framework ignore the cancellation token, so abort a stalled
                    // or cancelled download by disposing the response.
                    using (cts.Token.Register(() => response.Dispose()))
                    {
                        if (!response.IsSuccessStatusCode)
                            throw new HttpRequestException($"{info.Url} returned {(int)response.StatusCode} {response.ReasonPhrase}.");

                        var total = response.Content.Headers.ContentLength ?? 0;
                        using (var source = await response.Content.ReadAsStreamAsync())
                        using (var destination = File.Create(path))
                        using (var sha256 = SHA256.Create())
                        {
                            var buffer = new byte[64 * 1024];
                            var done = 0L;
                            while (true)
                            {
                                // Give up if no data arrives for 30 seconds.
                                cts.CancelAfter(TimeSpan.FromSeconds(30));
                                int read;
                                try
                                {
                                    read = await source.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                                }
                                catch (Exception) when (cts.IsCancellationRequested)
                                {
                                    ct.ThrowIfCancellationRequested();
                                    throw new TimeoutException("The download stopped responding.");
                                }
                                if (read == 0)
                                    break;
                                await destination.WriteAsync(buffer, 0, read, ct);
                                sha256.TransformBlock(buffer, 0, read, null, 0);
                                done += read;
                                progress?.Report((done, total));
                            }
                            sha256.TransformFinalBlock(buffer, 0, 0);

                            if (total != 0 && done != total)
                                throw new IOException($"The download was incomplete ({done} of {total} bytes).");
                            var digest = ToHex(sha256.Hash);
                            if (!string.Equals(digest, info.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
                                throw new InvalidDataException("The downloaded update failed its checksum. A newer build may have been published while downloading; try again.");
                        }
                    }
                }
                return path;
            }
            catch
            {
                TryDeleteDirectory(directory);
                throw;
            }
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Extracts a downloaded update into the staging folder next to the executable (on the same volume so
        /// that installing is a set of renames) and deletes the download. Returns the staging folder.
        /// </summary>
        public static string Stage(string zipPath)
        {
            // Each process stages separately so that another instance starting up does not remove it.
            var staging = Path.Combine(AppDirectory, StagingDirectoryPrefix + Process.GetCurrentProcess().Id);
            try
            {
                TryDeleteDirectory(staging);
                Directory.CreateDirectory(staging);
                var stagingRoot = staging + Path.DirectorySeparatorChar;
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var destination = Path.GetFullPath(Path.Combine(staging, entry.FullName));
                        if (!destination.StartsWith(stagingRoot, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException($"The update contains an invalid path: {entry.FullName}");
                        if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(destination);
                            continue;
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        entry.ExtractToFile(destination);
                    }
                }

                var exeName = Path.GetFileName(Application.ExecutablePath);
                if (!File.Exists(Path.Combine(staging, exeName)))
                    throw new InvalidDataException($"The update does not contain {exeName}.");
                return staging;
            }
            catch
            {
                TryDeleteDirectory(staging);
                throw;
            }
            finally
            {
                TryDeleteDirectory(Path.GetDirectoryName(zipPath));
            }
        }

        #endregion

        #region Install

        /// <summary>
        /// Installs the pending update and starts the new build. If installing fails, the current build is
        /// put back and restarted. Called once the main window has closed.
        /// </summary>
        public static void CompletePendingInstall()
        {
            var staging = PendingInstall;
            PendingInstall = null;
            try
            {
                Install(staging, AppDirectory);
            }
            catch (UpdateRollbackException ex)
            {
                // Some files could not be put back, so the current build may not start. The renamed files are
                // left for the user to recover from rather than being deleted on the next start.
                var result = MessageBox.Show(
                    $"{ex.Message}\n\nPeggleEdit may no longer start. Do you want to open the download page to download it again?",
                    "Failed to install update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                    Process.Start(Program.AppWebsite + "/releases");
                return;
            }
            catch (Exception ex)
            {
                TryDeleteDirectory(staging);
                MessageBox.Show(
                    $"{ex.Message}\n\nPeggleEdit has not been changed and will now restart.",
                    "Failed to install update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Restart(false);
                return;
            }
            Restart(true);
        }

        /// <summary>
        /// Moves every file from <paramref name="staging"/> into <paramref name="target"/>, renaming each file
        /// it replaces aside, and records the renamed files so the next start can delete them. Puts everything
        /// back if any file fails; throws <see cref="UpdateRollbackException"/> if that fails too.
        /// </summary>
        public static void Install(string staging, string target)
        {
            var token = Guid.NewGuid().ToString("N").Substring(0, 8);
            var stagingRoot = staging.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var renamed = new List<(string path, string oldPath)>();
            var installed = new List<(string source, string path)>();
            try
            {
                foreach (var source in Directory.GetFiles(staging, "*", SearchOption.AllDirectories))
                {
                    var path = Path.Combine(target, source.Substring(stagingRoot.Length));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    if (File.Exists(path))
                    {
                        var oldPath = $"{path}.{token}{OldFileSuffix}";
                        File.Move(path, oldPath);
                        renamed.Add((path, oldPath));
                    }
                    File.Move(source, path);
                    installed.Add((source, path));
                }
            }
            catch (Exception ex)
            {
                var failed = new List<string>();
                // Move new files back rather than deleting them so nothing is lost if a later step fails.
                foreach (var (source, path) in installed)
                {
                    if (!Retry(() => File.Move(path, source)) && !Retry(() => File.Delete(path)))
                        failed.Add(path);
                }
                for (var i = renamed.Count - 1; i >= 0; i--)
                {
                    var (path, oldPath) = renamed[i];
                    if (!Retry(() => File.Move(oldPath, path)))
                        failed.Add(path);
                }
                if (failed.Count != 0)
                {
                    throw new UpdateRollbackException(
                        $"{ex.Message}\n\nThe following files could not be restored:\n{string.Join("\n", failed.Distinct())}", ex);
                }
                throw;
            }

            // The update is installed at this point, so failing to record the renamed files only leaves them behind.
            try
            {
                File.AppendAllLines(Path.Combine(target, OldFilesManifestName), renamed.Select(x => x.oldPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to record replaced files: {ex.Message}");
            }
            TryDeleteDirectory(staging);
        }

        /// <summary>
        /// Runs <paramref name="action"/>, retrying briefly in case another process (e.g. a virus scanner)
        /// has the file open. Returns false if it still fails.
        /// </summary>
        private static bool Retry(Action action)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    if (attempt == 4)
                    {
                        Debug.WriteLine(ex.Message);
                        return false;
                    }
                    Thread.Sleep(250);
                }
            }
        }

        private static void Restart(bool updated)
        {
            try
            {
                var startInfo = new ProcessStartInfo(Application.ExecutablePath)
                {
                    UseShellExecute = false,
                    WorkingDirectory = AppDirectory
                };
                startInfo.EnvironmentVariables[RestartPidVariable] = Process.GetCurrentProcess().Id.ToString();
                if (updated)
                    startInfo.EnvironmentVariables[UpdatedVariable] = "1";
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to restart PeggleEdit", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Removes files left over from an update. Called at startup; if this process was started by the
        /// updater, waits for the previous process to exit so its files can be deleted.
        /// Returns true if this process was started by a successful update.
        /// </summary>
        public static bool Cleanup()
        {
            var pidText = Environment.GetEnvironmentVariable(RestartPidVariable);
            var updated = Environment.GetEnvironmentVariable(UpdatedVariable) == "1";
            Environment.SetEnvironmentVariable(RestartPidVariable, null);
            Environment.SetEnvironmentVariable(UpdatedVariable, null);

            if (int.TryParse(pidText, out var pid))
            {
                try
                {
                    using (var process = Process.GetProcessById(pid))
                    {
                        process.WaitForExit(30000);
                    }
                }
                catch (ArgumentException)
                {
                    // Already exited
                }
            }

            try
            {
                // Files that can not be deleted yet (e.g. another instance of the old build is still running)
                // stay in the manifest and are tried again on the next start.
                if (File.Exists(OldFilesManifestPath))
                {
                    var remaining = File.ReadAllLines(OldFilesManifestPath)
                        .Where(x => x.EndsWith(OldFileSuffix, StringComparison.OrdinalIgnoreCase) && File.Exists(x))
                        .Where(x => !TryDeleteFile(x))
                        .ToArray();
                    if (remaining.Length == 0)
                        File.Delete(OldFilesManifestPath);
                    else
                        File.WriteAllLines(OldFilesManifestPath, remaining);
                }

                // Staging folders of other running instances are left alone.
                foreach (var directory in Directory.GetDirectories(AppDirectory, StagingDirectoryPrefix + "*"))
                {
                    var suffix = Path.GetFileName(directory).Substring(StagingDirectoryPrefix.Length);
                    if (!int.TryParse(suffix, out var owner) || !IsProcessRunning(owner))
                        TryDeleteDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to clean up after update: {ex.Message}");
            }
            return updated;
        }

        private static bool IsProcessRunning(int pid)
        {
            try
            {
                using (var process = Process.GetProcessById(pid))
                {
                    return !process.HasExited;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to delete {path}: {ex.Message}");
                return false;
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to delete {path}: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// An update failed to install and some of the replaced files could not be put back.
    /// </summary>
    internal class UpdateRollbackException : IOException
    {
        public UpdateRollbackException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
