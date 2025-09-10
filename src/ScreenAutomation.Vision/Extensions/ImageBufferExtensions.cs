namespace ScreenAutomation.Vision.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using OpenCvSharp;
    using ScreenAutomation.Core;

    // Converts between Core's ImageBuffer and OpenCvSharp Mat (grayscale only).
    public static class ImageBufferExtensions
    {
        // Copy ImageBuffer (8-bit grayscale) into a new Mat (CV_8UC1), row-by-row to be stride-safe.
        public static Mat ToGrayMat(this ImageBuffer buffer)
        {
            if (buffer.Pixels is null)
                throw new ArgumentNullException(nameof(buffer.Pixels));

            var mat = new Mat(buffer.Height, buffer.Width, MatType.CV_8UC1);

            for (int y = 0; y < buffer.Height; y++)
            {
                var dstRowPtr = mat.Ptr(y);
                Marshal.Copy(buffer.Pixels, y * buffer.Width, dstRowPtr, buffer.Width);
            }

            return mat;
        }

        // Copy an OpenCV grayscale Mat (CV_8UC1) into an ImageBuffer (managed), accounting for stride.
        public static ImageBuffer FromGrayMat(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC1)
                throw new ArgumentException($"Expected CV_8UC1 mat, got {mat.Type()}");

            int width = mat.Cols;
            int height = mat.Rows;
            var pixels = new byte[width * height];

            if (mat.IsContinuous())
            {
                Marshal.Copy(mat.Data, pixels, 0, pixels.Length);
                return new ImageBuffer(width, height, pixels);
            }

            for (int y = 0; y < height; y++)
            {
                var srcRowPtr = mat.Ptr(y);
                Marshal.Copy(srcRowPtr, pixels, y * width, width);
            }

            return new ImageBuffer(width, height, pixels);
        }
    }
}
