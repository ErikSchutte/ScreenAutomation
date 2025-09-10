namespace ScreenAutomation.Core.Abstractions
{
    using System.Collections.Generic;

    // Orchestrates one capture → run the detection pipeline → return detections.
    public interface ICaptureRunner<TAspect>
    {
        IReadOnlyList<Detection<TAspect>> RunOnce();
    }
}
