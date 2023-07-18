using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "")]
    public struct Paper {

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool UseCustomPaper;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal Width;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal Height;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public bool UseCustomMargins;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MarginTop;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MarginRight;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MarginBottom;

        /// <summary>
        /// 
        /// </summary>
        [OSStructureField(Description = "")]
        public decimal MarginLeft;
    }
}
