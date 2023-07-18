using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "")]
    public struct Environment {
        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string BaseURL;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Locale;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public string Timezone;
    }
}
