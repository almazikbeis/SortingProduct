using SortingProduct.Domain.Entities;

namespace SortingProduct.Application.Abstractions;

public interface IProductBatchRepository
{
    Task AddRangeAsync(IEnumerable<ProductBatch> batches, CancellationToken cancellationToken);

    Task<List<ProductBatch>> GetAvailableForGroupingAsync(CancellationToken cancellationToken);

    Task<List<ProductBatch>> GetAllAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
