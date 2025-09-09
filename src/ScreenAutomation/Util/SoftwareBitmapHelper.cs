namespace ScreenAutomation.Util
{
    using System.Drawing;
    using Windows.Graphics.Imaging;
    using WinRT;
    using System.IO;

    public static class SoftwareBitmapHelper
    {
        // Convert System.Drawing.Bitmap -> Windows.Graphics.Imaging.SoftwareBitmap for OCR
        public static SoftwareBitmap FromBitmap(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Position = 0;
            var decoder = BitmapDecoder.CreateAsync(ms.AsRandomAccessStream()).AsTask().GetAwaiter().GetResult();
            return decoder.GetSoftwareBitmapAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
