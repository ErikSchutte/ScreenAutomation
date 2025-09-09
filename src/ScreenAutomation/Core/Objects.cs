namespace ScreenAutomation.Core
{
    public record DetectedObject(
        string Id,                 // stable id (e.g., "btn_ok")
        string Kind,               // e.g., "button", "icon", "badge"
        string CanonicalName,      // e.g., "OkButton"
        int X, int Y, int Width, int Height,
        double Confidence,
        double TimestampSeconds
    );
}
