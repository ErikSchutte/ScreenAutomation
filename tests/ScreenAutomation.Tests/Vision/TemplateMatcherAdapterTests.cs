namespace ScreenAutomation.Tests.Vision
{
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Vision.Extensions;
    using ScreenAutomation.Vision.Services;
    using Xunit;

    public class TemplateMatcherAdapterTests
    {
        [Fact]
        public void Match_returns_expected_location_and_score()
        {
            using var sceneMat = new Mat(120, 120, MatType.CV_8UC1, Scalar.All(0));
            var rect = new Rect(25, 40, 16, 12);
            sceneMat.Rectangle(rect, Scalar.All(255), thickness: -1);
            using var templateMat = new Mat(sceneMat, rect);

            var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
            var template = ImageBufferExtensions.FromGrayMat(templateMat);

            var matcher = new TemplateMatcher();
            var (box, score) = matcher.Match(scene, template);

            Assert.True(score > 0.9f);
            Assert.Equal(16, box.Width);
            Assert.Equal(12, box.Height);
            Assert.InRange(box.X, 23, 27);
            Assert.InRange(box.Y, 38, 42);
        }
    }
}
