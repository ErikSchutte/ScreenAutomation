namespace ScreenAutomation.App
{
    using System;
    using System.Threading.Tasks;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using ScreenAutomation.Core;

    public sealed class AutomationEngine
    {
        private readonly IScreenCapture   _capture;
        private readonly ITemplateMatcher _matcher;
        private readonly IOcrReader       _ocr;
        private readonly IInputController _input;
        private readonly IFeatureStore    _store;
        private readonly IObjectDetector _detector;

        public string LastOcrText { get; private set; } = string.Empty;
        public IReadOnlyList<DetectedObject> LastObjects { get; private set; } = Array.Empty<DetectedObject>();

        public AutomationEngine(IScreenCapture capture, ITemplateMatcher matcher, IOcrReader ocr,
                                IInputController input, IFeatureStore store)
        {
            _capture = capture;
            _matcher = matcher;
            _ocr     = ocr;
            _input   = input;
            _store   = store;

            var catalog = new ScreenAutomation.Vision.DiskTemplateCatalog("templates");
            _detector = new ScreenAutomation.Vision.ObjectDetector(catalog, _matcher, _store);
        }

        public async Task StepAsync(Mat? templateGray /* legacy single-template still accepted */)
        {
            using var bmp = _capture.CaptureDesktop();
            using var frame = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp); // BGR

            var ts = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;

            LastObjects = await _detector.DetectAsync(frame, ts);
            if (templateGray != null &&
                frame.Width >= templateGray.Width && frame.Height >= templateGray.Height)
            {
                var (btnRect, score) = _matcher.Find(frame, templateGray, 0.85);
                if (btnRect is not null && score > 0.9)
                {
                    var cx = btnRect.Value.X + btnRect.Value.Width / 2;
                    var cy = btnRect.Value.Y + btnRect.Value.Height / 2;
                    _input.Click(cx, cy);
                    Console.WriteLine($"[legacy] Clicked template at ({cx},{cy}) (score {score:F2})");
                }
            }

            var roi = new OpenCvSharp.Rect(10, 10, Math.Min(400, frame.Width - 10), Math.Min(120, frame.Height - 10));
            using var roiMat = new Mat(frame, roi);
            using var roiBmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(roiMat);
            LastOcrText = (await _ocr.ReadTextAsync(roiBmp)).Trim();
            if (!string.IsNullOrWhiteSpace(LastOcrText))
                _store.InsertSignal(ts, "ocr_roi", LastOcrText);
        }
    }
}
