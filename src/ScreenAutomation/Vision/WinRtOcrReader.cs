namespace ScreenAutomation.Vision
{
    using System.Threading.Tasks;
    using System.Drawing;
    using Windows.Media.Ocr;
    using ScreenAutomation.Core;
    using ScreenAutomation.Util;

    public sealed class WinRtOcrReader : IOcrReader
    {
        private readonly OcrEngine? _engine = OcrEngine.TryCreateFromUserProfileLanguages();

        public async Task<string> ReadTextAsync(Bitmap roiBitmap)
        {
            if (_engine is null) return string.Empty;
            var soft = SoftwareBitmapHelper.FromBitmap(roiBitmap);
            var res = await _engine.RecognizeAsync(soft);
            return res?.Text ?? string.Empty;
        }
    }
}
