namespace ScreenAutomation.Tests.Smoke
{
    using Xunit;

    // Canary: verifies test infra is wired before deeper slices.
    public class RepoBootsTests
    {
        [Fact]
        public void Repo_builds_and_tests_run()
        {
            Assert.True(true);
        }
    }
}
