using OpenCvSharp;
using ScreenAutomation.Core;

namespace ScreenAutomation.Vision.Extensions;


// Helpers to convert between Core's ImageBuffer (library-agnostic) and OpenCvSharp Mat.
// We keep things grayscale-only for now to keep the minimal slice simple.
public static class ImageBufferExtensions
{
    // Wrap an ImageBuffer (grayscale) in an OpenCV Mat (CV_8UC1). No deep copy is performed.

    public static Mat ToGrayMat(this ImageBuffer buffer)
    {
        // Mat uses the provided memory; ensure the buffer stays alive for the Mat's lifetime.
        return new Mat(buffer.Height, buffer.Width, MatType.CV_8UC1, buffer.Pixels);
    }


    // Copy an OpenCV grayscale Mat (CV_8UC1) into an ImageBuffer (managed).
    public static ImageBuffer FromGrayMat(Mat mat)
    {
        if (mat.Type() != MatType.CV_8UC1)
        {
            throw new ArgumentException($"Expected CV_8UC1 mat, got {mat.Type()}");
        }

        var w = mat.Cols;
        var h = mat.Rows;
        var total = w * h;
        var pixels = new byte[total];

        // GetArray performs a copy from Mat into managed memory.
        mat.GetArray(0, 0, pixels);

        return new ImageBuffer(w, h, pixels);
    }
}
