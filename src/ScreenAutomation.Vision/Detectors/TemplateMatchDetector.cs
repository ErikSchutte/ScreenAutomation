using OpenCvSharp;
using ScreenAutomation.Core;
using ScreenAutomation.Core.Abstractions;
using ScreenAutomation.Vision.Extensions;

namespace ScreenAutomation.Vision.Detectors;


// Extremely simple template matcher using OpenCV's MatchTemplate (CCoeffNormed).
// This is a baseline to prove the pipeline & test rig; we will harden it over time.
public sealed class TemplateMatchDetector : IAspectDetector<BoundingBox>, IDisposable
{
    private readonly Mat _template;     // CV_8UC1
    private readonly int _tw;
    private readonly int _th;

    /// <param name="template">Grayscale template pixels (Width*Height == Pixels.Length).</param>
    public TemplateMatchDetector(ImageBuffer template)
    {
        _template = template.ToGrayMat(); // wraps the array; we keep it alive in this class
        _tw = template.Width;
        _th = template.Height;
    }


    // Perform normalized template matching against the given scene (grayscale).
    public Detection<BoundingBox> Detect(ImageBuffer image)
    {
        using var scene = image.ToGrayMat();

        // result map size = (scene - template + 1)
        var resW = scene.Cols - _tw + 1;
        var resH = scene.Rows - _th + 1;

        if (resW <= 0 || resH <= 0)
        {
            // Template can't fit → trivially "no match"
            var degenerate = new BoundingBox(0, 0, _tw, _th);
            return new Detection<BoundingBox>(degenerate, degenerate, 0f);
        }

        using var result = new Mat(resH, resW, MatType.CV_32FC1);

        // Use CCoeffNormed which tends to be robust for simple luminance changes.
        Cv2.MatchTemplate(scene, _template, result, TemplateMatchModes.CCoeffNormed);

        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

        var box = new BoundingBox(maxLoc.X, maxLoc.Y, _tw, _th);
        return new Detection<BoundingBox>(box, box, (float)maxVal);
    }

    public void Dispose()
    {
        _template?.Dispose();
    }
}
