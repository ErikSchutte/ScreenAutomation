namespace ScreenAutomation.Tests.Vision
{
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Vision.Detectors;
    using ScreenAutomation.Vision.Extensions;
    using Xunit;

    // Generates images in-memory; no file assets needed.
    public class TemplateMatchDetectorTests
    {
        [Fact]
        public void Detect_returns_box_where_template_is_found()
        {
            using var sceneMat = new Mat(200, 200, MatType.CV_8UC1, Scalar.All(0));
            var rect = new Rect(80, 90, 50, 50);
            sceneMat.Rectangle(rect, Scalar.All(255), thickness: -1);
            using var templateMat = new Mat(sceneMat, rect);

            var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
            var template = ImageBufferExtensions.FromGrayMat(templateMat);

            var detector = new TemplateMatchDetector(template);
            var detection = detector.Detect(scene);

            Assert.True(detection.Score > 0.95f);
            Assert.Equal(50, detection.Box.Width);
            Assert.Equal(50, detection.Box.Height);
            Assert.InRange(detection.Box.X, 78, 82);
            Assert.InRange(detection.Box.Y, 88, 92);
        }

        [Fact]
        public void Detect_returns_low_score_when_template_is_absent()
        {
            using var sceneMat = new Mat(120, 120, MatType.CV_8UC1, Scalar.All(0));
            using var templateMat = new Mat(20, 20, MatType.CV_8UC1, Scalar.All(255));

            var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
            var template = ImageBufferExtensions.FromGrayMat(templateMat);

            var detector = new TemplateMatchDetector(template);
            var detection = detector.Detect(scene);

            Assert.True(detection.Score < 0.2f);
            Assert.Equal(20, detection.Box.Width);
            Assert.Equal(20, detection.Box.Height);
        }
    }
}
