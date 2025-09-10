namespace ScreenAutomation.Vision.Detectors
{
    using System;
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;
    using ScreenAutomation.Vision.Extensions;

    // Minimal template matcher (CCoeffNormed). Baseline slice to exercise the pipeline.
    public sealed class TemplateMatchDetector : IAspectDetector<BoundingBox>, IDisposable
    {
        private readonly Mat templateMat;   // CV_8UC1
        private readonly int tw;
        private readonly int th;

        public TemplateMatchDetector(ImageBuffer template)
        {
            this.templateMat = template.ToGrayMat();
            this.tw = template.Width;
            this.th = template.Height;
        }

        public Detection<BoundingBox> Detect(ImageBuffer image)
        {
            using var scene = image.ToGrayMat();

            var resW = scene.Cols - this.tw + 1;
            var resH = scene.Rows - this.th + 1;

            if (resW <= 0 || resH <= 0)
            {
                var box = new BoundingBox(0, 0, this.tw, this.th);
                return new Detection<BoundingBox>(box, box, 0f);
            }

            using var result = new Mat(resH, resW, MatType.CV_32FC1);

            Cv2.MatchTemplate(scene, this.templateMat, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

            var detected = new BoundingBox(maxLoc.X, maxLoc.Y, this.tw, this.th);
            return new Detection<BoundingBox>(detected, detected, (float)maxVal);
        }

        public void Dispose()
        {
            this.templateMat?.Dispose();
        }
    }
}
