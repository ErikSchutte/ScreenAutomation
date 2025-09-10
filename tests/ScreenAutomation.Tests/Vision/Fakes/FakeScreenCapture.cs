namespace ScreenAutomation.Tests.Vision.Fakes
{
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;

    internal sealed class FakeScreenCapture : IScreenCapture
    {
        private readonly ImageBuffer buffer;

        public FakeScreenCapture(ImageBuffer buffer)
        {
            this.buffer = buffer;
        }

        public ImageBuffer Capture() => this.buffer;
    }
}
