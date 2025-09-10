using FluentAssertions;
using Xunit;

namespace ScreenAutomation.Tests.Smoke;

// This is a canary test to ensure the solution builds & the test runner is wired.
public class RepoBootsTests
{
    [Fact]
    public void Repo_builds_and_tests_run()
    {
        true.Should().BeTrue("the test infrastructure should be alive before we refactor anything");
    }
}
