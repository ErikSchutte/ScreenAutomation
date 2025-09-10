using OpenCvSharp;
using ScreenAutomation.Core;
using ScreenAutomation.Storage;
using ScreenAutomation.Vision;
using Xunit;
using System.IO;
using System.Text.Json;

public class ObjectDetectorTests
{
    [Fact]
    public async void Detects_Template_From_Catalog()
    {
        // Arrange: build templates dir and a synthetic template/image
        var tempDir = Path.Combine(Path.GetTempPath(), "sa_templates_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        // synthetic haystack 200x200 with white rect at (30,40) size 20x10
        using var haystack = new Mat(new Size(200, 200), MatType.CV_8UC1, new Scalar(0));
        Cv2.Rectangle(haystack, new Rect(30, 40, 20, 10), new Scalar(255), -1);

        // save template exact crop
        using var template = new Mat(haystack, new Rect(30, 40, 20, 10));
        var tplName = "rect_20x10.png";
        Cv2.ImWrite(Path.Combine(tempDir, tplName), template);

        // catalog file
        var specs = new[] {
            new TemplateSpec { Id="rect_small", Kind="shape", CanonicalName="WhiteRect20x10", File=tplName, Threshold=0.95 }
        };
        File.WriteAllText(Path.Combine(tempDir, "templates.json"),
            JsonSerializer.Serialize(specs, new JsonSerializerOptions { WriteIndented = true }));

        var catalog = new DiskTemplateCatalog(tempDir);
        var matcher = new TemplateMatcher();
        var store   = new SqliteFeatureStore("Data Source=:memory:");
        store.Init();

        var sut = new ObjectDetector(catalog, matcher, store);

        // Act
        var ts = 123.456;
        var results = await sut.DetectAsync(haystack, ts);

        // Assert
        Assert.Single(results);
        var obj = results[0];
        Assert.Equal("rect_small", obj.Id);
        Assert.Equal(30, obj.X);
        Assert.Equal(40, obj.Y);
        Assert.Equal(20, obj.Width);
        Assert.Equal(10, obj.Height);
        Assert.True(obj.Confidence >= 0.95);
        Assert.Equal(ts, obj.TimestampSeconds);
    }
}
