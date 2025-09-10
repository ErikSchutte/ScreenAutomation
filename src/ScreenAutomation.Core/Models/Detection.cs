namespace ScreenAutomation.Core;

public sealed record Detection<T>(T Value, BoundingBox Box, float Score);
