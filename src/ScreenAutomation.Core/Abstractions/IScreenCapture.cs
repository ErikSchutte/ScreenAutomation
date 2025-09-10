namespace ScreenAutomation.Core.Abstractions
{
    using ScreenAutomation.Core;

    public interface IScreenCapture
    {
        ImageBuffer Capture();
    }
}
