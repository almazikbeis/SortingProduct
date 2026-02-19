using SortingProduct.Application.Abstractions;
using SortingProduct.Domain.Entities;

namespace SortingProduct.Application.Services;

public sealed class ProductImportService
{
    private readonly IExcelProductImporter _excelProductImporter;
    private readonly IProductBatchRepository _productBatchRepository;

    public ProductImportService(IExcelProductImporter excelProductImporter, IProductBatchRepository productBatchRepository)
    {
        _excelProductImporter = excelProductImporter;
        _productBatchRepository = productBatchRepository;
    }

    public async Task<int> ImportAsync(Stream xlsxStream, CancellationToken cancellationToken)
    {
        var importedRows = await _excelProductImporter.ParseAsync(xlsxStream, cancellationToken);

        var productBatches = importedRows
            .Where(row => row.Quantity > 0)
            .Select(row => new ProductBatch
            {
                Id = Guid.NewGuid(),
                Name = row.Name.Trim(),
                Unit = row.Unit.Trim(),
                UnitPriceEur = row.UnitPriceEur,
                InitialQuantity = row.Quantity,
                RemainingQuantity = row.Quantity,
                Status = ProductBatchStatus.New,
                CreatedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        if (productBatches.Count == 0)
        {
            return 0;
        }

        await _productBatchRepository.AddRangeAsync(productBatches, cancellationToken);
        await _productBatchRepository.SaveChangesAsync(cancellationToken);

        return productBatches.Count;
    }
}
