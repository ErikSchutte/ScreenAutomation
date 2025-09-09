namespace ScreenAutomation.Core
{
    public record DetectionResult(string ElementId, int X, int Y, int W, int H, double Confidence);
}
