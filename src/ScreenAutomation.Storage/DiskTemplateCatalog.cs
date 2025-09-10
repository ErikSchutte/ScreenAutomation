namespace ScreenAutomation.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using OpenCvSharp;
    using ScreenAutomation.Core;
    using ScreenAutomation.Core.Abstractions;

    // Scans a directory for images and exposes them as TemplateSpec entries.
    public sealed class DiskTemplateCatalog : ITemplateCatalog
    {
        private readonly string root;
        private readonly List<TemplateSpec> templates;

        public DiskTemplateCatalog(string rootFolder)
        {
            root = Path.GetFullPath(rootFolder);
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"Template root not found: {root}");

            // allowed image types; extend as needed
            var patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp" };

            var files = patterns.SelectMany(p => Directory.EnumerateFiles(root, p, SearchOption.AllDirectories))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                .ToList();

            templates = new List<TemplateSpec>(files.Count);

            foreach (var file in files)
            {
                // Use OpenCV to read dimensions without pulling in other libs
                using var mat = Cv2.ImRead(file, ImreadModes.Grayscale);
                if (mat.Empty())
                    continue; // skip corrupt/unreadable files

                var name = Path.GetFileNameWithoutExtension(file);
                templates.Add(new TemplateSpec(name, file, mat.Cols, mat.Rows));
            }
        }

        public IReadOnlyList<TemplateSpec> List() => templates;

        public TemplateSpec? GetByName(string name)
        {
            return templates.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
