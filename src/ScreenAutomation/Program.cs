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
using ScreenAutomation.Vision;

internal class Program
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

        // Optional: load a template from file (if present)
        Mat? template = null;
        const string templatePath = "template.png";
        if (System.IO.File.Exists(templatePath))
            template = Cv2.ImRead(templatePath, ImreadModes.Grayscale);

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

    static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "…";
}
