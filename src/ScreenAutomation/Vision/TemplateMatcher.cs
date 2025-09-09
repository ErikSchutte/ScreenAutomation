namespace ScreenAutomation.Vision
{
    using OpenCvSharp;
    using ScreenAutomation.Core;

    public sealed class TemplateMatcher : ITemplateMatcher
    {
        public (Rect? Region, double Score) Find(Mat haystackBgrOrGray, Mat templateGray, double threshold = 0.85)
        {
            using var gray = new Mat();
            if (haystackBgrOrGray.Channels() == 3)
                Cv2.CvtColor(haystackBgrOrGray, gray, ColorConversionCodes.BGR2GRAY);
            else
                haystackBgrOrGray.CopyTo(gray);

            using var result = new Mat(gray.Rows - templateGray.Rows + 1,
                                       gray.Cols - templateGray.Cols + 1,
                                       MatType.CV_32FC1);

            Cv2.MatchTemplate(gray, templateGray, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            if (maxVal >= threshold)
            {
                var rect = new Rect(maxLoc.X, maxLoc.Y, templateGray.Width, templateGray.Height);
                return (rect, maxVal);
            }
            return (null, 0);
        }
    }
}
