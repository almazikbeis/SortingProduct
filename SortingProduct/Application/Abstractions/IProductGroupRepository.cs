using SortingProduct.Domain.Entities;

namespace SortingProduct.Application.Abstractions;

public interface IProductGroupRepository
{
    Task AddAsync(ProductGroup group, CancellationToken cancellationToken);

    Task<List<ProductGroup>> GetGroupsAsync(int skip, int take, CancellationToken cancellationToken);

    Task<ProductGroup?> GetGroupWithItemsAsync(Guid groupId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
