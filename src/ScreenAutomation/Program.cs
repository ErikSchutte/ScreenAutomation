using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using ScreenAutomation.App;
using ScreenAutomation.Capture;
using ScreenAutomation.Core;
using ScreenAutomation.Input;
using ScreenAutomation.Storage;
using ScreenAutomation.Util;
using ScreenAutomation.Catalog;
using ScreenAutomation.Vision;

internal sealed class Program
{
    static async Task Main()
    {
        Console.WriteLine("Screen Automation MVP — GDI capture running… (ESC to stop)");

        // Wire up concrete services (simple manual DI for now)
        IScreenCapture    capture   = new GdiScreenCapture();
        ITemplateMatcher  matcher   = new TemplateMatcher();
        IOcrReader        ocr       = new WinRtOcrReader();
        IInputController  input     = new Win32InputController();
        IFeatureStore     store     = new SqliteFeatureStore("automation.db");
        var engine = new AutomationEngine(capture, matcher, ocr, input, store);

        store.Init();

        // Load templates from /templates using templates.json
        var catalog = TemplateCatalogRuntime.Load();
        var matcher  = new TemplateMatcher();

        // Choose id via env var SA_TEMPLATE_ID, or first id that starts with "character", or the first id.
        var tplId = Environment.GetEnvironmentVariable("SA_TEMPLATE_ID")
                ?? catalog.FindFirstIdStartingWith("character")
                ?? catalog.Ids.FirstOrDefault();

        CharacterDetector? charDetector = null;
        if (!string.IsNullOrWhiteSpace(tplId))
        {
            charDetector = new CharacterDetector(matcher, catalog, tplId!, 0.90);
            Console.WriteLine($"Template id: {tplId} (from {catalog.Root})");
        }

        var loop = new ActionLoop();
        var sw = Stopwatch.StartNew();

        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
        {
            try
            {
                await engine.StepAsync(template);

                // Diagnostics
                loop.Tick();
                if (loop.ShouldPrint())
                {
                    var found = engine.LastObjects;
                    var summary = found.Count == 0
                        ? "none"
                        : string.Join(", ", found.Select(o => $"{o.Id}@{o.X},{o.Y}({o.Confidence:F2})"));
                    Console.WriteLine($"FPS ~{loop.Fps:F1} | Found: {summary} | OCR: {Truncate(engine.LastOcrText, 60)}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Frame processing error: " + ex.Message);
            }

            await Task.Delay(10); // light throttle
        }
    }
    static string Truncate(string s, int n)
        => s.Length <= n ? s : string.Concat(s.AsSpan(0, n), "…");
}
