namespace ScreenAutomation.Tests.Vision
{
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Vision.Detectors;
    using ScreenAutomation.Vision.Extensions;
    using ScreenAutomation.Vision.Pipeline;
    using Xunit;

    public class DetectionPipelineTests
    {
        [Fact]
        public void Pipeline_runs_detectors_and_returns_detections()
        {
            // Scene with a 40x30 bright rectangle at roughly (60,70).
            using var sceneMat = new Mat(160, 160, MatType.CV_8UC1, Scalar.All(0));
            var rect = new Rect(60, 70, 40, 30);
            sceneMat.Rectangle(rect, Scalar.All(255), thickness: -1);
            using var templateMat = new Mat(sceneMat, rect);

            var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
            var template = ImageBufferExtensions.FromGrayMat(templateMat);

            var detector = new TemplateMatchDetector(template);
            var pipeline = new DetectionPipeline<BoundingBox>(new[] { detector });

            var results = pipeline.Run(scene);

            Assert.Single(results);
            var detection = results[0];

            Assert.True(detection.Score > 0.9f);
            Assert.Equal(40, detection.Box.Width);
            Assert.Equal(30, detection.Box.Height);
            Assert.InRange(detection.Box.X, 58, 62);
            Assert.InRange(detection.Box.Y, 68, 72);
        }
    }
}
