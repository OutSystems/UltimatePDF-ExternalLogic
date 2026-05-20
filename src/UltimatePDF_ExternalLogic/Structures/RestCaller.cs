using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// Rest PDF Store information. Required for the external logic to call the API.
    /// </summary>
    [OSStructure(Description = "Rest PDF Store information. Required for the external logic to call the API.")]
    public struct RestCaller {

        /// <summary>
        /// Rest call authentication token
        /// </summary>
        [OSStructureField(Description = "Rest call authentication token")]
        public string Token;
        
        /// <summary>
        /// Base URL
        /// </summary>
        [OSStructureField(Description = "Base URL")]
        public string BaseUrl;

        /// <summary>
        /// Rest API definition module name
        /// </summary>
        [OSStructureField(Description = "Rest API definition module name")]
        public string Module;

        /// <summary>
        /// Store PDF action REST Path
        /// </summary>
        [OSStructureField(Description = "Store PDF action REST Path")]
        public string StorePath;

        /// <summary>
        /// Log zip action REST Path
        /// </summary>
        [OSStructureField(Description = "Log zip action REST Path")]
        public string LogPath;

        /// <summary>
        /// REST client call timeout in seconds
        /// </summary>
        [OSStructureField(Description = "REST client call timeout in seconds")]
        public int timeoutSeconds;
    }
}
