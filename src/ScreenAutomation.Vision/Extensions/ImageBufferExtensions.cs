namespace ScreenAutomation.Vision.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using OpenCvSharp;
    using ScreenAutomation.Core;

    // Converts between Core's ImageBuffer and OpenCvSharp Mat (grayscale only for now).
    public static class ImageBufferExtensions
    {
        // Copy ImageBuffer (8-bit grayscale) into a new Mat (CV_8UC1).
        public static Mat ToGrayMat(this ImageBuffer buffer)
        {
            if (buffer.Pixels is null)
                throw new ArgumentNullException(nameof(buffer.Pixels));

            var mat = new Mat(buffer.Height, buffer.Width, MatType.CV_8UC1);
            var length = buffer.Pixels.Length;

            // Copy managed bytes -> unmanaged Mat buffer
            Marshal.Copy(buffer.Pixels, 0, mat.Data, length);
            return mat;
        }

        // Copy an OpenCV grayscale Mat (CV_8UC1) into an ImageBuffer (managed).
        public static ImageBuffer FromGrayMat(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC1)
                throw new ArgumentException($"Expected CV_8UC1 mat, got {mat.Type()}");

            var width = mat.Cols;
            var height = mat.Rows;
            var pixels = new byte[width * height];

            // Copy unmanaged Mat buffer -> managed bytes
            Marshal.Copy(mat.Data, pixels, 0, pixels.Length);

            return new ImageBuffer(width, height, pixels);
        }
    }
}
