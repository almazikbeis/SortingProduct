using Microsoft.EntityFrameworkCore;
using SortingProduct.Application.Abstractions;
using SortingProduct.Domain.Entities;
using SortingProduct.Infrastructure.Persistence;

namespace SortingProduct.Infrastructure.Repositories;

public sealed class ProductGroupRepository : IProductGroupRepository
{
    private readonly AppDbContext _dbContext;

    public ProductGroupRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ProductGroup group, CancellationToken cancellationToken) =>
        _dbContext.ProductGroups.AddAsync(group, cancellationToken).AsTask();

    public Task<List<ProductGroup>> GetGroupsAsync(int skip, int take, CancellationToken cancellationToken) =>
        _dbContext.ProductGroups
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<ProductGroup?> GetGroupWithItemsAsync(Guid groupId, CancellationToken cancellationToken) =>
        _dbContext.ProductGroups
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(i => i.ProductBatch)
            .FirstOrDefaultAsync(x => x.Id == groupId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
