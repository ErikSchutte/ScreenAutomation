using System;
using System.Collections.Generic;
using OpenCvSharp;
using ScreenAutomation.Catalog;
using ScreenAutomation.Core;

namespace ScreenAutomation.Vision
{
    /// <summary>Character detection using a logical template id from the runtime catalog.</summary>
    public sealed class CharacterDetector : IDisposable
    {
        private readonly ITemplateMatcher _matcher;
        private readonly List<Mat> _templates; // owned here
        private readonly double _threshold;
        private readonly double[] _scales = new[] { 0.90, 0.95, 1.00, 1.05, 1.10 };

        public string TemplateId { get; }

        public CharacterDetector(ITemplateMatcher matcher, TemplateCatalogRuntime catalog, string templateId, double threshold = 0.90)
        {
            _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
            TemplateId = templateId ?? throw new ArgumentNullException(nameof(templateId));
            _threshold = threshold;

            var mats = catalog.GetTemplates(templateId);
            if (mats.Count == 0)
                throw new InvalidOperationException($"No images found for template id '{templateId}' in {catalog.Root}");

            // Clone into our own ownership to simplify lifetime
            _templates = new List<Mat>(mats.Count);
            foreach (var m in mats) _templates.Add(m.Clone());
        }

        /// <summary>Returns best region and score in [0..1] or (null,score) if below threshold.</summary>
        public (Rect? Region, double Score) Detect(Mat frameBgr)
        {
            if (frameBgr.Empty()) return (null, 0);

            Rect? best = null;
            double bestScore = 0;

            foreach (var baseTpl in _templates)
            {
                foreach (var s in _scales)
                {
                    using var tpl = Scale(baseTpl, s);
                    if (tpl.Width > frameBgr.Width || tpl.Height > frameBgr.Height) continue;

                    var (region, score) = _matcher.Find(frameBgr, tpl, _threshold);
                    if (region.HasValue && score > bestScore)
                    {
                        best = region;
                        bestScore = score;
                    }
                }
            }

            return (best, bestScore);
        }

        private static Mat Scale(Mat srcGray, double scale)
        {
            if (Math.Abs(scale - 1.0) < 1e-6) return srcGray.Clone();
            var dst = new Mat();
            var w = Math.Max(8, (int)Math.Round(srcGray.Width * scale));
            var h = Math.Max(8, (int)Math.Round(srcGray.Height * scale));
            Cv2.Resize(srcGray, dst, new Size(w, h), 0, 0, InterpolationFlags.Area);
            return dst;
        }

        public void Dispose()
        {
            foreach (var m in _templates) m.Dispose();
            _templates.Clear();
        }
    }
}
