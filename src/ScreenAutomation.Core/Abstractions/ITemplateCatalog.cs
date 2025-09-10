namespace ScreenAutomation.Core.Abstractions
{
    using System.Collections.Generic;

    // Read-only listing of templates (e.g., files on disk).
    public interface ITemplateCatalog
    {
        IReadOnlyList<TemplateSpec> List();

        TemplateSpec? GetByName(string name);
    }
}
