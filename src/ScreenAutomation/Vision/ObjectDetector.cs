namespace ScreenAutomation.Vision
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OpenCvSharp;
    using ScreenAutomation.Core;

    public sealed class ObjectDetector : IObjectDetector
    {
        private readonly ITemplateCatalog _catalog;
        private readonly ITemplateMatcher _matcher;
        private readonly IFeatureStore _store;

        public ObjectDetector(ITemplateCatalog catalog, ITemplateMatcher matcher, IFeatureStore store)
        {
            _catalog = catalog;
            _matcher = matcher;
            _store   = store;
        }

        public Task<IReadOnlyList<DetectedObject>> DetectAsync(Mat frameBgr, double tsSeconds)
        {
            var list = new List<DetectedObject>();

            foreach (var spec in _catalog.Templates)
            {
                using var tpl = _catalog.LoadTemplateMat(spec);
                if (tpl is null) continue;

                if (frameBgr.Width < tpl.Width || frameBgr.Height < tpl.Height) continue;

                var (rect, score) = _matcher.Find(frameBgr, tpl, spec.Threshold);
                if (rect is null) continue;

                // Persist minimal observation and add domain object
                _store.UpsertElement(spec.Id, spec.Kind, spec.CanonicalName);
                _store.InsertObservation(tsSeconds, "window:Desktop", spec.Id,
                    rect.Value.X, rect.Value.Y, rect.Value.Width, rect.Value.Height,
                    "visible", score, null);

                list.Add(new DetectedObject(
                    spec.Id, spec.Kind, spec.CanonicalName,
                    rect.Value.X, rect.Value.Y, rect.Value.Width, rect.Value.Height,
                    score, tsSeconds
                ));
            }

            return Task.FromResult<IReadOnlyList<DetectedObject>>(list);
        }
    }
}
