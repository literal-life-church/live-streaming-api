﻿namespace LiteralLifeChurch.LiveStreamingApi.Models.Workflow
{
    public class ResourceNamesModel : IWorkflowModel
    {

        public string AssetName { get; set; }

        public string LiveOutputName { get; set; }

        public string ManifestName { get; set; }

        public string StreamingLocatorName { get; set; }
    }
}
