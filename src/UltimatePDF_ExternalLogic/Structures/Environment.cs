using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// Environment information
    /// </summary>
    [OSStructure(Description = "Environment information")]
    public struct Environment {
        /// <summary>
        /// Environment Base URL
        /// </summary>
        [OSStructureField(Description = "Environment Base URL")]
        public string BaseURL;

        /// <summary>
        /// Browser configured locale
        /// </summary>
        [OSStructureField(Description = "Browser configured locale")]
        public string Locale;

        /// <summary>
        /// Browser configured timezone
        /// </summary>
        [OSStructureField(Description = "Browser configured timezone")]
        public string Timezone;
    }
}
