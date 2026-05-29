using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {
    
    [OSStructure(Description = "Screen viewport configuration")]
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
