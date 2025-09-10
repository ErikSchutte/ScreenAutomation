using OpenCvSharp;
using ScreenAutomation.Core;

namespace ScreenAutomation.Vision
{
    // Deterministic template matcher using SqDiffNormed (min is best).
    public sealed class TemplateMatcher : ITemplateMatcher
    {
        // Matches interface: haystack can be BGR or Gray; template should be Gray.
        public (Rect? Region, double Score) Find(Mat haystackBgrOrGray, Mat templateGray, double threshold = 0.85)
        {
            using var h = ToGray8U(haystackBgrOrGray);
            using var t = ToGray8U(templateGray);

            using var result = new Mat();
            Cv2.MatchTemplate(h, t, result, TemplateMatchModes.SqDiffNormed);
            Cv2.MinMaxLoc(result, out var minVal, out _, out var minLoc, out _);

            // Convert "distance" to a confidence in [0..1]
            var conf = 1.0 - minVal;
            if (conf < threshold)
                return (null, conf);

            var rect = new Rect(minLoc.X, minLoc.Y, t.Width, t.Height);
            return (rect, conf);
        }

        // Legacy helper used by tests: return the top-left of the best match.
        public static Point FindTopLeft(Mat sourceBgr, Mat templateBgr)
        {
            using var h = ToGray8U(sourceBgr);
            using var t = ToGray8U(templateBgr);

            using var result = new Mat();
            Cv2.MatchTemplate(h, t, result, TemplateMatchModes.SqDiffNormed);
            Cv2.MinMaxLoc(result, out _, out _, out var minLoc, out _);
            return minLoc;
        }

        private static Mat ToGray8U(Mat src)
        {
            if (src.Empty())
                return src.Clone();

            Mat gray;
            if (src.Channels() == 1)
                gray = src.Clone();
            else
            {
                gray = new Mat();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            }

            // Ensure 8-bit depth (tests may create non-8U mats)
            if (gray.Depth() != MatType.CV_8U)
            {
                var norm = new Mat();
                Cv2.Normalize(gray, norm, 0, 255, NormTypes.MinMax);
                var u8 = new Mat();
                norm.ConvertTo(u8, MatType.CV_8U);
                norm.Dispose();
                gray.Dispose();
                return u8;
            }

            return gray;
        }
    }
}
