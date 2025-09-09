using System.IO;
using System.Text.Json;
using OpenCvSharp;
using ScreenAutomation.Core;
using ScreenAutomation.Vision;
using Xunit;

public class TemplateCatalogTests
{
    [Fact]
    public void Loads_Template_And_Image()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sa_tpl_" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);

        // Make a tiny white square as "char_A"
        using var tpl = new Mat(new Size(10,10), MatType.CV_8UC1, new Scalar(255));
        var file = Path.Combine(dir, "char_A.png");
        Cv2.ImWrite(file, tpl);

        var specs = new[] { new TemplateSpec { Id="char_A", Kind="glyph", CanonicalName="A", File="char_A.png", Threshold=0.85 } };
        File.WriteAllText(Path.Combine(dir, "templates.json"), JsonSerializer.Serialize(specs));

        var catalog = new DiskTemplateCatalog(dir);
        Assert.Single(catalog.Templates);

        using var loaded = catalog.LoadTemplateMat(catalog.Templates[0]);
        Assert.NotNull(loaded);
        Assert.Equal(10, loaded!.Width);
        Assert.Equal(10, loaded.Height);
    }
}
