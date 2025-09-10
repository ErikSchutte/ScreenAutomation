namespace ScreenAutomation.Core
{
    // Result when matching a single template against a scene.
    public sealed class TemplateMatchResult
    {
        public TemplateMatchResult(TemplateSpec template, BoundingBox box, float score)
        {
            this.Template = template;
            this.Box = box;
            this.Score = score;
        }

        public TemplateSpec Template { get; }

        public BoundingBox Box { get; }

        public float Score { get; }
    }
}
