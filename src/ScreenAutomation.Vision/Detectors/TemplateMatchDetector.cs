namespace ScreenAutomation.Vision.Detectors
{
    using System;
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;
    using ScreenAutomation.Vision.Extensions;

    // Minimal template matcher using SQDIFF_NORMED; invert the min value to a [0..1] score.
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

            // SQDIFF_NORMED is robust when the template has zero variance (e.g., solid color).
            Cv2.MatchTemplate(scene, this.templateMat, result, TemplateMatchModes.SqDiffNormed);

            Cv2.MinMaxLoc(result, out double minVal, out _, out Point minLoc, out _);

            // Convert "lower is better" to a confidence-like score.
            var score = 1f - (float)minVal;

            var detected = new BoundingBox(minLoc.X, minLoc.Y, this.tw, this.th);
            return new Detection<BoundingBox>(detected, detected, score);
        }

        public void Dispose()
        {
            this.templateMat?.Dispose();
        }
    }
}
