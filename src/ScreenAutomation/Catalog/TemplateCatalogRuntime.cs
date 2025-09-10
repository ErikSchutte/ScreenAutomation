using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenCvSharp;

namespace ScreenAutomation.Catalog
{
    /// <summary>Runtime loader for templates.json + images under /templates.</summary>
    public sealed class TemplateCatalogRuntime : IDisposable
    {
        public string Root { get; }
        private readonly Dictionary<string, List<string>> _idToPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Mat> _cache = new(); // owns loaded Mats

        private TemplateCatalogRuntime(string root) => Root = root;

        public static TemplateCatalogRuntime Load(string? preferredRoot = null)
        {
            // Detect templates root: bin\...\templates, otherwise working dir\templates
            var candidates = new[]
            {
                preferredRoot,
                Path.Combine(AppContext.BaseDirectory, "templates"),
                Path.Combine(Environment.CurrentDirectory, "templates")
            }.Where(p => !string.IsNullOrWhiteSpace(p))
             .Select(p => Path.GetFullPath(p!))
             .Distinct();

            foreach (var root in candidates)
            {
                if (Directory.Exists(root))
                {
                    var cat = new TemplateCatalogRuntime(root);
                    cat.LoadIndex();
                    return cat;
                }
            }

            throw new DirectoryNotFoundException("templates folder not found near the executable.");
        }

        public string? FindFirstIdStartingWith(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return _idToPaths.Keys.FirstOrDefault();
            return _idToPaths.Keys.FirstOrDefault(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyCollection<string> Ids => _idToPaths.Keys;

        /// <summary>Load all images for a logical id as grayscale Mats (owned by this catalog).</summary>
        public IReadOnlyList<Mat> GetTemplates(string id)
        {
            if (!_idToPaths.TryGetValue(id, out var paths) || paths.Count == 0)
                return Array.Empty<Mat>();

            var mats = new List<Mat>(paths.Count);
            foreach (var p in paths)
            {
                var abs = Path.IsPathRooted(p) ? p : Path.Combine(Root, p);
                if (!File.Exists(abs)) continue;
                var m = Cv2.ImRead(abs, ImreadModes.Grayscale);
                if (!m.Empty()) { mats.Add(m); _cache.Add(m); }
            }
            return mats;
        }

        private void LoadIndex()
        {
            // 1) templates.json if present (supports a few common shapes)
            var jsonPath = Path.Combine(Root, "templates.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    using var fs = File.OpenRead(jsonPath);
                    using var doc = JsonDocument.Parse(fs);
                    ParseJson(doc.RootElement);
                }
                catch { /* graceful fallback to enumeration */ }
            }

            // 2) Enumerate images when not covered by json (id = relative subpath without extension)
            var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp" };
            foreach (var file in Directory.EnumerateFiles(Root, "*.*", SearchOption.AllDirectories))
            {
                if (!exts.Contains(Path.GetExtension(file))) continue;
                var rel = Path.GetRelativePath(Root, file);
                var id = Path.ChangeExtension(rel, null)!.Replace('\\', '/'); // keep logical “folder/name”
                if (!_idToPaths.ContainsKey(id)) _idToPaths[id] = new List<string>();
                if (!_idToPaths[id].Contains(rel)) _idToPaths[id].Add(rel);
            }
        }

        private void ParseJson(JsonElement root)
        {
            // Accept shapes:
            // A) { "templates":[ {"id":"foo","image":"x.png"} ... ] }
            // B) { "items":[ {"id":"foo","images":[...]} ... ] }
            // C) { "id": {"path":"x.png"} } (dictionary)
            // D) { "id": "x.png" } (dictionary short form)
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("templates", out var arr) || root.TryGetProperty("items", out arr))
                {
                    if (arr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var e in arr.EnumerateArray())
                            AddFromObject(e);
                    }
                }
                else
                {
                    foreach (var prop in root.EnumerateObject())
                    {
                        var id = prop.Name;
                        var val = prop.Value;
                        if (val.ValueKind == JsonValueKind.String)
                        {
                            Add(id, val.GetString()!);
                        }
                        else if (val.ValueKind == JsonValueKind.Object)
                        {
                            AddFromObject(val, idOverride: id);
                        }
                    }
                }
            }
        }

        private void AddFromObject(JsonElement obj, string? idOverride = null)
        {
            string? id = idOverride;
            string? image = null;
            string? path = null;
            string? folder = null;
            List<string>? images = null;

            if (!id.HasValue() && obj.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                id = idProp.GetString();

            if (obj.TryGetProperty("image", out var imgProp) && imgProp.ValueKind == JsonValueKind.String)
                image = imgProp.GetString();
            if (obj.TryGetProperty("path", out var pathProp) && pathProp.ValueKind == JsonValueKind.String)
                path = pathProp.GetString();
            if (obj.TryGetProperty("folder", out var folderProp) && folderProp.ValueKind == JsonValueKind.String)
                folder = folderProp.GetString();
            if (obj.TryGetProperty("images", out var imgsProp) && imgsProp.ValueKind == JsonValueKind.Array)
                images = imgsProp.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToList();

            if (!id.HasValue()) return;

            if (image.HasValue()) Add(id!, image!);
            if (path.HasValue()) Add(id!, path!);
            if (folder.HasValue())
            {
                var dir = Path.IsPathRooted(folder!) ? folder! : Path.Combine(Root, folder!);
                if (Directory.Exists(dir))
                {
                    foreach (var f in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        var ext = Path.GetExtension(f);
                        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                            ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
                        {
                            var rel = Path.GetRelativePath(Root, f);
                            Add(id!, rel);
                        }
                    }
                }
            }
            if (images != null)
            {
                foreach (var i in images) Add(id!, i);
            }
        }

        private void Add(string id, string relativeOrAbsolutePath)
        {
            if (!_idToPaths.TryGetValue(id, out var list))
                _idToPaths[id] = list = new List<string>();
            if (!list.Contains(relativeOrAbsolutePath))
                list.Add(relativeOrAbsolutePath);
        }

        public void Dispose()
        {
            foreach (var m in _cache) m.Dispose();
            _cache.Clear();
        }
    }

    internal static class StringExt
    {
        public static bool HasValue(this string? s) => !string.IsNullOrWhiteSpace(s);
    }
}
