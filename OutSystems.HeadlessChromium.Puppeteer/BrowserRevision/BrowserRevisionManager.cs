using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using OutSystems.HeadlessChromium.Puppeteer.BrowserProducts;
using OutSystems.HeadlessChromium.Puppeteer.Utils;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserRevision {
    public class BrowserRevisionManager {

        private static readonly object browserSetupLock = new object();
        private static readonly HashSet<FaultyRevisionInfo> faultyRevisions = new HashSet<FaultyRevisionInfo>();

        private readonly BrowserProduct product;
        private readonly DirectoryInfo temp;
        private BrowserFetcher fetcher;



        private sealed class FaultyRevisionInfo {
            private readonly DirectoryInfo folder;
            private readonly DateTime created;

            public FaultyRevisionInfo(RevisionInfo revisionInfo) {
                this.folder = new DirectoryInfo(revisionInfo.FolderPath);
                this.created = this.folder.CreationTimeUtc;
            }

            public bool CanBeDiscarded {
                get { return !folder.Exists || folder.CreationTimeUtc != created; }
            }

            public override int GetHashCode() {
                return folder.GetHashCode() ^ created.GetHashCode();
            }

            public override bool Equals(object obj) {
                var o = obj as FaultyRevisionInfo;
                return o != null && folder == o.folder && created == o.created;
            }
        }



        public BrowserRevisionManager(DirectoryInfo temp) {
            this.temp = temp;
        }



        public DirectoryInfo Temp {
            get { return temp; }
        }

        private BrowserFetcher Fetcher {
            get {
                if (fetcher == null) {
                    fetcher = new BrowserFetcher(new BrowserFetcherOptions() {
                        Product = product.Product,
                        Path = Path.Combine(temp.FullName, product.RevisionFolder)
                    });
                }

                return fetcher;
            }
        }

        public sealed class DownloadRevisionResult {
            public readonly bool WasUnhealthy;
            public readonly bool DownloadedNewRevision;

            public DownloadRevisionResult(bool wasUnhealthy, bool downloadedNewRevision) {
                this.WasUnhealthy = wasUnhealthy;
                this.DownloadedNewRevision = downloadedNewRevision;
            }
        }


        private string CombineZipEntryPath(string destination, string zipEntryPath) {
            // Mitigates Zip Slip vulnerability
            string combined = Path.Combine(destination, zipEntryPath);
            if (!combined.StartsWith(destination)) {
                throw new IOException($"Invalid path in zip entry: {zipEntryPath}");
            }

            return combined;
        }

        private string GetFolderPath(string revision) {
            return Path.Combine(Fetcher.DownloadsFolder, $"{Fetcher.Platform}-{revision}");
        }


        public bool IsHealthyRevision(RevisionInfo revision) {
            return IsHealthyRevision(revision, out _);
        }

        public bool IsHealthyRevision(RevisionInfo revision, out string reason) {
            lock (faultyRevisions) {
                var discarded = faultyRevisions.Where(p => p.CanBeDiscarded).ToArray();
                foreach (var toDiscard in discarded) {
                    faultyRevisions.Remove(toDiscard);
                }

                if (faultyRevisions.Contains(new FaultyRevisionInfo(revision))) {
                    reason = "Browser startup has failed previously";
                    return false;
                }
            }

            try {
                product.AssertHealthyRevision(revision);
                reason = null;
                return true;
            } catch (IOException e) {
                faultyRevisions.Add(new FaultyRevisionInfo(revision));
                reason = e.Message;
                return false;
            }
        }


        public void OnFaultyRevision(RevisionInfo revision) {
            lock (faultyRevisions) {
                faultyRevisions.Add(new FaultyRevisionInfo(revision));
            }
        }
    }
}
