using System.Net;

namespace QuotesApi.Tests;

public class CancellationTests
{
    [Fact]
    public async Task GetCollection_WhenRequestIsCancelled_OperationIsCancelled()
    {
        await using var factory = new CustomWebApplicationFactory();

        using var client = factory.CreateClient();

        using var cts = new CancellationTokenSource();

        var requestTask = client.GetAsync(
            "/api/collections/1",
            cts.Token);

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await requestTask);
    }
}