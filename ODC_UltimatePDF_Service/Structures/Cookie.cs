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
    public struct Cookie {
        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Value;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool HttpOnly;
    }
}
