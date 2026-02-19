using Microsoft.EntityFrameworkCore;
using SortingProduct.Application.Abstractions;
using SortingProduct.Domain.Entities;
using SortingProduct.Infrastructure.Persistence;

namespace SortingProduct.Infrastructure.Repositories;

public sealed class ProductBatchRepository : IProductBatchRepository
{
    private readonly AppDbContext _dbContext;

    public ProductBatchRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddRangeAsync(IEnumerable<ProductBatch> batches, CancellationToken cancellationToken) =>
        _dbContext.ProductBatches.AddRangeAsync(batches, cancellationToken);

    public Task<List<ProductBatch>> GetAvailableForGroupingAsync(CancellationToken cancellationToken) =>
        _dbContext.ProductBatches
            .Where(x => x.RemainingQuantity > 0 && x.Status != ProductBatchStatus.Processed)
            .OrderByDescending(x => x.UnitPriceEur)
            .ToListAsync(cancellationToken);

    public Task<List<ProductBatch>> GetAllAsync(CancellationToken cancellationToken) =>
        _dbContext.ProductBatches
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
