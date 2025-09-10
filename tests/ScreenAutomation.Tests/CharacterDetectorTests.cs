using OpenCvSharp;
using ScreenAutomation.Core;
using ScreenAutomation.Storage;
using ScreenAutomation.Vision;
using Xunit;
using System.IO;
using System.Text.Json;

public class CharacterDetectorTests
{
    [Fact]
    public async void Detects_Char_A_In_Synthetic_Image()
    {
        // 1) Build a fake template catalog on disk
        var dir = Path.Combine(Path.GetTempPath(), "sa_char_" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);

        // A crisp 12x16 "A" block for testing
        using var A = new Mat(new Size(12,16), MatType.CV_8UC1, new Scalar(0));
        Cv2.PutText(A, "A", new OpenCvSharp.Point(0,14), HersheyFonts.HersheySimplex, 0.6, new Scalar(255), 2, LineTypes.AntiAlias);
        var aName = "char_A.png"; Cv2.ImWrite(Path.Combine(dir, aName), A);

        File.WriteAllText(Path.Combine(dir, "templates.json"),
            JsonSerializer.Serialize(new[] {
                new TemplateSpec { Id="char_A", Kind="glyph", CanonicalName="A", File=aName, Threshold=0.80 }
            }));

        var catalog = new DiskTemplateCatalog(dir);
        var matcher = new TemplateMatcher();
        var store   = new SqliteFeatureStore("Data Source=:memory:"); store.Init();
        var detector = new ObjectDetector(catalog, matcher, store);

        // 2) Create haystack and stamp the glyph at (40,60)
        using var img = new Mat(new Size(200,200), MatType.CV_8UC1, new Scalar(0));
        A.CopyTo(new Mat(img, new Rect(40,60, A.Width, A.Height)));

        // 3) Detect
        var results = await detector.DetectAsync(img, 123.0);

        Assert.Single(results);
        Assert.Equal("char_A", results[0].Id);
        Assert.Equal(40, results[0].X);
        Assert.Equal(60, results[0].Y);
        Assert.True(results[0].Confidence >= 0.80);
    }
}
