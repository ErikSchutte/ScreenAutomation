using System.IO;
using System.Text.Json;
using ScreenAutomation.Core;
using ScreenAutomation.Vision;
using Xunit;

public class TemplateSmokeTests
{
    [Fact]
    public void Catalog_Loads_From_Executable_Directory()
    {
        // Arrange: create a temp templates folder next to the test bin
        var baseDir = AppContext.BaseDirectory;
        var tplDir = Path.Combine(baseDir, "templates");
        Directory.CreateDirectory(tplDir);
        File.WriteAllText(Path.Combine(tplDir, "templates.json"),
            JsonSerializer.Serialize(new[] {
                new TemplateSpec { Id="smoke", Kind="glyph", CanonicalName="Smoke", File="x.png", Threshold=0.8 }
            })
        );
        File.WriteAllBytes(Path.Combine(tplDir, "x.png"), new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // minimal PNG header (invalid image but fine for presence)

        // Act
        var catalog = new DiskTemplateCatalog(); // default "templates" relative to EXE
        var count = catalog.Templates.Count;

        // Assert
        Assert.True(count >= 1);
    }
}
