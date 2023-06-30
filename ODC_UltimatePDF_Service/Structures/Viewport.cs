using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ODC_UltimatePDF_Service.Structures {
    
    [OSStructure(Description = "")]
    public struct Viewport {
        
        /// <summary>
        /// 
        /// </summary>
        [OSStructureField]
        public int Width;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField] 
        public int Height;
    }
}
