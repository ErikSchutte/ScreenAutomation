namespace ScreenAutomation.Core.Abstractions;

public interface IAspectDetector<TAspect>
{
    Detection<TAspect> Detect(ImageBuffer image);
}
