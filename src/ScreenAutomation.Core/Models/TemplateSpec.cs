namespace ScreenAutomation.Core
{
    // Simple data for a template on disk.
    public sealed class TemplateSpec
    {
        public TemplateSpec(string name, string filePath, int width, int height)
        {
            this.Name = name;
            this.FilePath = filePath;
            this.Width = width;
            this.Height = height;
        }

        public string Name { get; }

        public string FilePath { get; }

        public int Width { get; }

        public int Height { get; }
    }
}
