namespace ScreenAutomation.Vision
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using OpenCvSharp;
    using ScreenAutomation.Core;

    public sealed class DiskTemplateCatalog : ITemplateCatalog
    {
        private readonly string _rootDir;   // e.g., "./templates"
        private readonly string _indexFile; // e.g., "./templates/templates.json"
        private List<TemplateSpec> _templates = new();

        public DiskTemplateCatalog(string rootDir = "templates")
        {
            var baseDir = AppContext.BaseDirectory; // e.g., bin\Debug\net9.0-windows...
            _rootDir = Path.IsPathRooted(rootDir) ? rootDir : Path.Combine(baseDir, rootDir);
            _indexFile = Path.Combine(_rootDir, "templates.json");
            if (!Directory.Exists(_rootDir)) Directory.CreateDirectory(_rootDir);

            Console.WriteLine($"[Catalog] Loaded {_templates.Count} template spec(s) from {_indexFile}");
            Reload();
        }

        public IReadOnlyList<TemplateSpec> Templates => _templates;

        public void Reload()
        {
            _templates.Clear();
            if (!File.Exists(_indexFile))
            {
                // Create a starter file if missing
                var starter = new[] {
                    new TemplateSpec { Id="btn_template", Kind="button", CanonicalName="TemplateButton", File="template.png", Threshold=0.88 }
                };
                Directory.CreateDirectory(_rootDir);
                File.WriteAllText(_indexFile, JsonSerializer.Serialize(starter, new JsonSerializerOptions { WriteIndented = true }));
                _templates = starter.ToList();
                return;
            }

            var json = File.ReadAllText(_indexFile);
            var arr = JsonSerializer.Deserialize<List<TemplateSpec>>(json) ?? new();
            _templates = arr;
        }

        public Mat? LoadTemplateMat(TemplateSpec spec)
        {
            var path = Path.Combine(_rootDir, spec.File);
            if (!File.Exists(path)) return null;
            var img = Cv2.ImRead(path, ImreadModes.Grayscale);
            return img;
        }
    }
}
