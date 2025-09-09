namespace ScreenAutomation.Core
{
    using OpenCvSharp;
    using System.Drawing;
    using System.Threading.Tasks;

    public interface IScreenCapture
    {
        Bitmap CaptureDesktop();
    }

    public interface ITemplateMatcher
    {
        (Rect? Region, double Score) Find(Mat haystackBgrOrGray, Mat templateGray, double threshold = 0.85);
    }

    public interface IOcrReader
    {
        Task<string> ReadTextAsync(Bitmap roiBitmap);
    }

    public interface IInputController
    {
        void Click(int x, int y);
    }

    public interface IFeatureStore
    {
        void Init();
        void UpsertElement(string id, string kind, string canonical);
        void InsertObservation(double ts, string windowId, string elementId,
                               int x, int y, int w, int h, string state, double conf, string? text);
        void InsertSignal(double ts, string key, string value);
    }
}
