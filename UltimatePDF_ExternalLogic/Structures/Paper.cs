using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "Render page properties definition of the print to PDF in the browser.")]
    public struct Paper {

        /// <summary>
        /// Print on custom paper
        /// </summary>
        [OSStructureField(Description = "Print on custom paper")]
        public bool UseCustomPaper;

        /// <summary>
        /// Custom paper width
        /// </summary>
        [OSStructureField(Description = "Custom paper width")]
        public decimal Width;

        /// <summary>
        /// Custom paper height
        /// </summary>
        [OSStructureField(Description = "Custom paper height")]
        public decimal Height;

        /// <summary>
        /// Print with a custom margin
        /// </summary>
        [OSStructureField(Description = "Print with a custom margin")]
        public bool UseCustomMargins;

        /// <summary>
        /// Custom margin top
        /// </summary>
        [OSStructureField(Description = "Custom margin top")]
        public decimal MarginTop;

        /// <summary>
        /// Custom margin right
        /// </summary>
        [OSStructureField(Description = "Custom margin right")]
        public decimal MarginRight;

        /// <summary>
        /// Custom margin bottom
        /// </summary>
        [OSStructureField(Description = "Custom margin bottom")]
        public decimal MarginBottom;

        /// <summary>
        /// Custom margin left
        /// </summary>
        [OSStructureField(Description = "Custom margin left")]
        public decimal MarginLeft;
    }
}
