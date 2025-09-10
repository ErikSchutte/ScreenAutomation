namespace ScreenAutomation.Vision.Services
{
    using ScreenAutomation.Core;
    using ScreenAutomation.Vision.Detectors;

    // Small adapter that returns a tuple; keeps tests simple.
    public sealed class TemplateMatcher
    {
        public (BoundingBox Box, float Score) Match(ImageBuffer scene, ImageBuffer template)
        {
            using var detector = new TemplateMatchDetector(template);
            var detection = detector.Detect(scene);
            return (detection.Box, detection.Score);
        }
    }
}
