namespace ScreenAutomation.Vision.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;
    using ScreenAutomation.Vision.Extensions;

    // Preloads templates from a catalog, then matches all against a scene.
    public sealed class TemplateCatalogMatcher
    {
        private readonly List<(TemplateSpec Spec, ImageBuffer Buffer)> templates;

        public TemplateCatalogMatcher(ITemplateCatalog catalog)
        {
            // Preload ImageBuffers for speed/stability during matching.
            this.templates = new List<(TemplateSpec, ImageBuffer)>();
            foreach (var t in catalog.List())
            {
                if (!File.Exists(t.FilePath))
                {
                    continue;
                }

                using var mat = Cv2.ImRead(t.FilePath, ImreadModes.Grayscale);
                if (mat.Empty())
                {
                    continue;
                }

                var buffer = ImageBufferExtensions.FromGrayMat(mat);
                this.templates.Add((t, buffer));
            }
        }

        public IReadOnlyList<TemplateMatchResult> MatchAll(ImageBuffer scene)
        {
            var results = new List<TemplateMatchResult>(this.templates.Count);
            foreach (var (spec, buffer) in this.templates)
            {
                using var detector = new Detectors.TemplateMatchDetector(buffer);
                var detection = detector.Detect(scene);
                results.Add(new TemplateMatchResult(spec, detection.Box, detection.Score));
            }

            // Highest score first
            return results.OrderByDescending(r => r.Score).ToList();
        }

        public TemplateMatchResult? FindBest(ImageBuffer scene, float minScore = 0.75f)
        {
            var best = this.MatchAll(scene).FirstOrDefault();
            if (best == null || best.Score < minScore)
            {
                return null;
            }

            return best;
        }
    }
}
