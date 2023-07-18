using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// 
    /// </summary>
    [OSStructure(Description = "")]
    public struct Management_BrowserRevision {
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
        public string Path;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public DateTime Created;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal SizeMegabytes;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool IsHealthy;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string UnhealthyReason;
    }
}
