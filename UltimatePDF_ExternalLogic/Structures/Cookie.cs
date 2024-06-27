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
    [OSStructure(Description = "Browser cookie. To be set on the browser when rendering the page")]
    public struct Cookie {
        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "Cookie name")]
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "Cookie value")]
        public string Value;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "Set HTTP only cookie attribute")]
        public bool HttpOnly;
    }
}
