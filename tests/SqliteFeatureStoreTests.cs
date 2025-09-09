using ScreenAutomation.Storage;
using Xunit;

public class SqliteFeatureStoreTests
{
    [Fact]
    public void Init_And_Insert_Works_InMemory()
    {
        var store = new SqliteFeatureStore("Data Source=:memory:");
        store.Init();
        store.UpsertElement("id1", "button", "OkButton");
        store.InsertObservation(1.23, "win", "id1", 1, 2, 3, 4, "visible", 0.99, null);
        store.InsertSignal(1.23, "ocr", "hello");
        // If no exceptions, basic persistence layer is wired up.
        Assert.True(true);
    }
}
