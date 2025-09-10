namespace ScreenAutomation.Vision.Pipeline
{
    using System.Collections.Generic;
    using System.Linq;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;

    // Simple pipeline: runs each detector and returns all detections.
    public sealed class DetectionPipeline<TAspect> : IDetectionPipeline<TAspect>
    {
        private readonly IReadOnlyList<IAspectDetector<TAspect>> detectors;

        public DetectionPipeline(IEnumerable<IAspectDetector<TAspect>> detectors)
        {
            this.detectors = detectors?.ToList() ?? new List<IAspectDetector<TAspect>>();
        }

        public IReadOnlyList<Detection<TAspect>> Run(ImageBuffer image)
        {
            var results = new List<Detection<TAspect>>(this.detectors.Count);
            foreach (var d in this.detectors)
            {
                results.Add(d.Detect(image));
            }

            return results;
        }
    }
}
