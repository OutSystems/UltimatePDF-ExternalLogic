using System;
using Microsoft.Extensions.Logging;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// Troubleshooting log settings for a generation task.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="LogZipFiles"/> because a structure carrying a File field cannot be
    /// used as an input parameter: the ODC runtime fails to deserialize it even when the field is
    /// left unset. So the flags travel in on this type, and the File travels out on that one.
    /// </remarks>
    [OSStructure(Description = "Troubleshooting log settings for the task")]
    public struct LogOptions {

        [OSStructureField(DataType = OSDataType.Boolean,
            Description = "Collects execution logs. If False LogsZipFile will be empty")]
        public bool CollectLogs;

        [OSStructureField(DataType = OSDataType.Boolean,
            Description = "Attach PDF and HTML files to the logs")]
        public bool AttachFilesLogs;

        /// <summary>
        /// Builds the logger these settings describe.
        /// </summary>
        internal Logger CreateLogger(ILogger odcLogger) {
            return Logger.GetLogger(odcLogger, CollectLogs, AttachFilesLogs);
        }

        /// <summary>
        /// Stores the collected logs as a File, named after the artifact they describe so the two are
        /// recognisable as a pair.
        /// </summary>
        /// <remarks>
        /// <paramref name="store"/> is passed in rather than taken as a dependency so that this type
        /// stays a plain data contract with no knowledge of file storage. When nothing was collected
        /// the file is left null instead of storing an empty ZIP.
        /// </remarks>
        internal static LogZipFiles StoreZip(Logger logger, string artifactName,
                                      Func<string, string, byte[], IOSFile> store) {
            var zip = logger.GetZipFile();

            return new LogZipFiles {
                LogsZipFile = zip.Length == 0
                    ? null
                    : store(FileArtifacts.LogsFileNameFor(artifactName), FileArtifacts.ZipContentType, zip),
            };
        }
    }
}
