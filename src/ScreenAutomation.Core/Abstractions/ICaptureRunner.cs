namespace ScreenAutomation.Core.Abstractions
{
    using System.Collections.Generic;

    // Orchestrates one capture → runs the detection pipeline → returns detections.
    public interface ICaptureRunner<TAspect>
    {
        IReadOnlyList<Detection<TAspect>> RunOnce();
    }
}
