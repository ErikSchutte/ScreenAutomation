namespace ScreenAutomation.Tests.Vision
{
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Tests.Vision.Fakes;
    using ScreenAutomation.Vision.Detectors;
    using ScreenAutomation.Vision.Extensions;
    using ScreenAutomation.Vision.Pipeline;
    using ScreenAutomation.Vision.Runtime;
    using Xunit;

    public class CaptureRunnerTests
    {
        [Fact]
        public void RunOnce_returns_detections_from_pipeline()
        {
            using var sceneMat = new Mat(160, 160, MatType.CV_8UC1, Scalar.All(0));
            var rect = new Rect(32, 48, 24, 18);
            sceneMat.Rectangle(rect, Scalar.All(255), thickness: -1);
            using var templateMat = new Mat(sceneMat, rect);

            var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
            var template = ImageBufferExtensions.FromGrayMat(templateMat);

            var detector = new TemplateMatchDetector(template);
            var pipeline = new DetectionPipeline<BoundingBox>(new[] { detector });
            var capture = new FakeScreenCapture(scene);
            var runner = new CaptureRunner<BoundingBox>(capture, pipeline);

            var results = runner.RunOnce();

            Assert.Single(results);
            var detection = results[0];
            Assert.True(detection.Score > 0.9f);
            Assert.Equal(24, detection.Box.Width);
            Assert.Equal(18, detection.Box.Height);
            Assert.InRange(detection.Box.X, 30, 34);
            Assert.InRange(detection.Box.Y, 46, 50);
        }
    }
}
