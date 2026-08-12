using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Tests;

public class FakeCollectionRepository : ICollectionRepository
{
    public async Task<Collection?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await Task.Delay(
            Timeout.InfiniteTimeSpan,
            cancellationToken);

        return null;
    }

    public Task<Collection> AddAsync(
        Collection collection,
        CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task UpdateAsync(
        Collection collection,
        CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
        => throw new NotImplementedException();
}