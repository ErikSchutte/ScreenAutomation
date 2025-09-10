namespace ScreenAutomation.Tests.Vision
{
    using System;
    using System.IO;
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Storage;
    using ScreenAutomation.Vision.Extensions;
    using ScreenAutomation.Vision.Services;
    using Xunit;

    public class TemplateCatalogMatcherTests
    {
        [Fact]
        public void FindBest_picks_the_right_template_and_location()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "sa_match_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);

            try
            {
                // two templates on disk
                var fileA = Path.Combine(tmp, "coin.png");
                using (var m = new Mat(10, 10, MatType.CV_8UC1, Scalar.All(255)))
                {
                    Cv2.Circle(m, new Point(5, 5), 4, Scalar.All(128), thickness: 1);
                    Cv2.ImWrite(fileA, m);
                }

                var fileB = Path.Combine(tmp, "heart.png");
                using (var m = new Mat(8, 12, MatType.CV_8UC1, Scalar.All(0)))
                {
                    Cv2.Rectangle(m, new Rect(2, 2, 4, 8), Scalar.All(255), thickness: -1);
                    Cv2.ImWrite(fileB, m);
                }

                // scene contains heart at (40, 30)
                using var sceneMat = new Mat(100, 100, MatType.CV_8UC1, Scalar.All(0));
                sceneMat.Rectangle(new Rect(40, 30, 8, 12), Scalar.All(255), thickness: -1);

                var scene = ImageBufferExtensions.FromGrayMat(sceneMat);

                var catalog = new DiskTemplateCatalog(tmp);
                var matcher = new TemplateCatalogMatcher(catalog);

                var best = matcher.FindBest(scene, minScore: 0.8f);

                Assert.NotNull(best);
                Assert.Equal("heart", best!.Template.Name);
                Assert.Equal(8, best.Box.Width);
                Assert.Equal(12, best.Box.Height);
                Assert.InRange(best.Box.X, 38, 42);
                Assert.InRange(best.Box.Y, 28, 32);
                Assert.True(best.Score > 0.9f);
            }
            finally
            {
                try
                {
                    Directory.Delete(tmp, recursive: true);
                }
                catch
                {
                }
            }
        }
    }
}
