using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ODC_UltimatePDF_Service.Structures {

    /// <summary>
    /// 
    /// </summary>
    [OSStructure(Description = "")]
    public struct Management_Process {
        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public int ProcessId;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string ProcessName;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public DateTime ProcessStarted;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal ProcessorUsageSeconds;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MemoryUsageMegabytes;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public int ChildProcesses;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool IsUnmanageable;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string UnmanageableReason;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool IsOrphaned;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Product;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Revision;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public int JobCount;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public DateTime LastJobStarted;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public DateTime LastJobFinished;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal LastJobDurationSeconds;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MinJobDurationSeconds;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MaxJobDurationSeconds;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal AverageJobDurationSeconds;
    }
}
