using FluentAssertions;
using OpenCvSharp;
using ScreenAutomation.Core;
using ScreenAutomation.Vision.Detectors;
using ScreenAutomation.Vision.Extensions;
using Xunit;

namespace ScreenAutomation.Tests.Vision;

// These tests create tiny grayscale images in-memory to avoid shipping assets.
public class TemplateMatchDetectorTests
{
    [Fact]
    public void Detect_returns_box_where_template_is_found()
    {
        // Create a 200x200 black canvas (CV_8UC1 == grayscale).
        using var sceneMat = new Mat(200, 200, MatType.CV_8UC1, Scalar.All(0));

        // Draw a white 50x50 square at (x=80, y=90).
        var rect = new Rect(80, 90, 50, 50);
        sceneMat.Rectangle(rect, Scalar.All(255), thickness: -1);

        // Use the exact square as our template (best-case baseline for this detector).
        using var templateMat = new Mat(sceneMat, rect);

        // Convert Mats -> ImageBuffer (the Core-friendly pixel container).
        var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
        var template = ImageBufferExtensions.FromGrayMat(templateMat);

        var detector = new TemplateMatchDetector(template);

        var detection = detector.Detect(scene);

        detection.Score.Should().BeGreaterThan(0.95f, "perfect template match should be near 1.0");
        detection.Box.Width.Should().Be(50);
        detection.Box.Height.Should().Be(50);

        // Allow a few pixels of tolerance due to floating rounding when using OpenCV result maps.
        detection.Box.X.Should().BeInRange(78, 82);
        detection.Box.Y.Should().BeInRange(88, 92);
    }

    [Fact]
    public void Detect_returns_low_score_when_template_is_absent()
    {
        // Scene: all zeros; Template: all 255 (white). There is no match.
        using var sceneMat = new Mat(120, 120, MatType.CV_8UC1, Scalar.All(0));
        using var templateMat = new Mat(20, 20, MatType.CV_8UC1, Scalar.All(255));

        var scene = ImageBufferExtensions.FromGrayMat(sceneMat);
        var template = ImageBufferExtensions.FromGrayMat(templateMat);

        var detector = new TemplateMatchDetector(template);

        var detection = detector.Detect(scene);

        detection.Score.Should().BeLessThan(0.2f, "no matching region should yield a low normalized score");
        detection.Box.Width.Should().Be(20);
        detection.Box.Height.Should().Be(20);
    }
}
