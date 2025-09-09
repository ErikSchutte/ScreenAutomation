using OpenCvSharp;
using ScreenAutomation.Vision;
using Xunit;

public class TemplateMatcherTests
{
    [Fact]
    public void Finds_TopLeft_Template_In_Synthetic_Image()
    {
        // Create a synthetic 200x200 gray image with a white 20x10 rectangle at (30,40)
        using var img = new Mat(new OpenCvSharp.Size(200, 200), MatType.CV_8UC1, new Scalar(0));
        Cv2.Rectangle(img, new Rect(30, 40, 20, 10), new Scalar(255), -1);

        // Extract the exact template
        using var template = new Mat(img, new Rect(30, 40, 20, 10));

        var sut = new TemplateMatcher();
        var (rect, score) = sut.Find(img, template, 0.9);

        Assert.True(rect.HasValue);
        Assert.InRange(score, 0.9, 1.0001);
        Assert.Equal(30, rect!.Value.X);
        Assert.Equal(40, rect.Value.Y);
        Assert.Equal(20, rect.Value.Width);
        Assert.Equal(10, rect.Value.Height);
    }
}
