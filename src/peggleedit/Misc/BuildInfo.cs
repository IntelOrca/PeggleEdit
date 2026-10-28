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
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace IntelOrca.PeggleEdit.Designer
{
    /// <summary>
    /// The running build's channel and commit. CI bakes these in through the PeggleEditChannel,
    /// PeggleEditCommit and PeggleEditCommitDate MSBuild properties; a local build is a "dev" build
    /// and never updates itself.
    /// </summary>
    internal static class BuildInfo
    {
        public const string ReleaseChannel = "release";
        public const string NightlyChannel = "nightly";
        public const string DevChannel = "dev";

        public static string Channel { get; } = GetMetadata("PeggleEditChannel") ?? DevChannel;
        public static string Commit { get; } = GetMetadata("PeggleEditCommit") ?? "";
        public static DateTimeOffset? CommitDate { get; } = ParseDate(GetMetadata("PeggleEditCommitDate"));

        public static bool IsPublished => Channel == ReleaseChannel || Channel == NightlyChannel;
        public static bool IsNightly => Channel == NightlyChannel;
        public static string ShortCommit => ShortenCommit(Commit);

        /// <summary>
        /// The version suffix shown after the version name, e.g. "nightly 4f2a1c9". Empty for releases.
        /// </summary>
        public static string Suffix
        {
            get
            {
                if (Channel == ReleaseChannel)
                    return "";
                return string.IsNullOrEmpty(Commit) ? Channel : $"{Channel} {ShortCommit}";
            }
        }

        public static string ShortenCommit(string commit)
        {
            if (string.IsNullOrEmpty(commit))
                return "";
            return commit.Length > 7 ? commit.Substring(0, 7) : commit;
        }

        public static DateTimeOffset? ParseDate(string text)
        {
            if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                return date;
            return null;
        }

        private static string GetMetadata(string key)
        {
            var value = typeof(BuildInfo).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == key)
                ?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
