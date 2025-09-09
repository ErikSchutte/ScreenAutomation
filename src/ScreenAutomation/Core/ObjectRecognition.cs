namespace ScreenAutomation.Core
{
    using OpenCvSharp;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    // Describes a template entry in the catalog
    public sealed class TemplateSpec
    {
        public string Id { get; init; } = "";
        public string Kind { get; init; } = "unknown";
        public string CanonicalName { get; init; } = "";
        public string File { get; init; } = ""; // path under templates/
        public double Threshold { get; init; } = 0.88; // default
    }

    // Abstractions
    public interface ITemplateCatalog
    {
        IReadOnlyList<TemplateSpec> Templates { get; }
        void Reload(); // re-read disk
        Mat? LoadTemplateMat(TemplateSpec spec); // grayscale
    }

    public interface IObjectDetector
    {
        // Returns zero or more detected objects for given frame
        Task<IReadOnlyList<DetectedObject>> DetectAsync(Mat frameBgr, double tsSeconds);
    }
}
