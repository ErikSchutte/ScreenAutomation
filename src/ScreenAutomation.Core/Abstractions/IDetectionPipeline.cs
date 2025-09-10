namespace ScreenAutomation.Core.Abstractions
{
    using System.Collections.Generic;

    public interface IDetectionPipeline<TAspect>
    {
        IReadOnlyList<Detection<TAspect>> Run(ImageBuffer image);
    }
}
