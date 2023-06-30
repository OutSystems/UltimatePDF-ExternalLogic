using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ODC_UltimatePDF_Service.Structures {

    [OSStructure(Description = "")]
    public struct Management_TemporaryFile {

        [OSStructureField(Description = "")]
        public string Name;

        [OSStructureField(Description = "")]
        public string Path;

        [OSStructureField(Description = "")]
        public DateTime Created;

        [OSStructureField(Description = "")]
        public DateTime LastModified;

        [OSStructureField(Description = "")]
        public decimal SizeMegabytes;
    }
}
