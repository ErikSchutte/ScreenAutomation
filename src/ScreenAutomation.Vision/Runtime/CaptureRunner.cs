namespace ScreenAutomation.Vision.Runtime
{
    using System.Collections.Generic;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;

    // Grabs one frame from IScreenCapture and passes it to the pipeline.
    public sealed class CaptureRunner<TAspect> : ICaptureRunner<TAspect>
    {
        private readonly IScreenCapture capture;
        private readonly IDetectionPipeline<TAspect> pipeline;

        public CaptureRunner(IScreenCapture capture, IDetectionPipeline<TAspect> pipeline)
        {
            this.capture = capture;
            this.pipeline = pipeline;
        }

        public IReadOnlyList<Detection<TAspect>> RunOnce()
        {
            var frame = this.capture.Capture();
            return this.pipeline.Run(frame);
        }
    }
}
