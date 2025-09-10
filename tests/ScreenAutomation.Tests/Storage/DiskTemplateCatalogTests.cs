namespace ScreenAutomation.Tests.Storage
{
    using System;
    using System.IO;
    using OpenCvSharp;
    using ScreenAutomation.Storage;
    using Xunit;

    public class DiskTemplateCatalogTests
    {
        [Fact]
        public void Lists_png_templates_from_disk_with_dimensions()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "sa_catalog_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);

            try
            {
                var file1 = Path.Combine(tmp, "cursor.png");
                using (var m = new Mat(7, 11, MatType.CV_8UC1, Scalar.All(255)))
                {
                    Cv2.ImWrite(file1, m);
                }

                var sub = Path.Combine(tmp, "ui");
                Directory.CreateDirectory(sub);
                var file2 = Path.Combine(sub, "button.png");
                using (var m = new Mat(20, 30, MatType.CV_8UC1, Scalar.All(128)))
                {
                    Cv2.ImWrite(file2, m);
                }

                var catalog = new DiskTemplateCatalog(tmp);
                var list = catalog.List();

                Assert.True(list.Count >= 2);

                var cursor = catalog.GetByName("cursor");
                Assert.NotNull(cursor);
                Assert.EndsWith("cursor.png", cursor!.FilePath, StringComparison.OrdinalIgnoreCase);
                Assert.Equal(11, cursor.Width);
                Assert.Equal(7, cursor.Height);

                var button = catalog.GetByName("button");
                Assert.NotNull(button);
                Assert.Equal(30, button!.Width);
                Assert.Equal(20, button.Height);
            }
            finally
            {
                try
                {
                    Directory.Delete(tmp, recursive: true);
                }
                catch
                {
                    // ignore temp cleanup failures
                }
            }
        }
    }
}
