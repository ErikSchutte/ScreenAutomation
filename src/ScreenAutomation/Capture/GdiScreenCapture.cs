namespace ScreenAutomation.Capture
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using ScreenAutomation.Core;

    public sealed partial class GdiScreenCapture : IScreenCapture
    {
        public Bitmap CaptureDesktop()
        {
            var w = GetSystemMetrics(0);  // SM_CXSCREEN
            var h = GetSystemMetrics(1);  // SM_CYSCREEN

            var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, new DSize(w, h), CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }

        [LibraryImport("user32.dll")]
        private static partial int GetSystemMetrics(int nIndex);
    }
}
