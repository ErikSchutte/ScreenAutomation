namespace ScreenAutomation.Core.Abstractions;

public interface IDetectionPipeline
{
    IEnumerable<object> Run(ImageBuffer image);
}
