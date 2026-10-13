using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// The log file a generation task produced.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Output only. A structure carrying a File field cannot be used as an input parameter, so the
    /// settings arrive on <see cref="LogOptions"/> instead.
    /// </para>
    /// <para>
    /// The File is wrapped in a structure because ODC's generated code cannot bind a File to an
    /// <c>out</c> parameter directly — it declares the variable as its own internal OSFile class, and
    /// an <c>out</c> argument needs an identity type match. As a field of a structure it travels fine.
    /// </para>
    /// </remarks>
    [OSStructure(Description = "The logs file produced by the task")]
    public struct LogZipFiles {

        [OSStructureField(DataType = OSDataType.File,
            Description = "Task logs. Empty when CollectLogs is False")]
        public IOSFile? LogsZipFile;
    }
}
