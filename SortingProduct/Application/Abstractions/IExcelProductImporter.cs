using SortingProduct.Application.Dtos;

namespace SortingProduct.Application.Abstractions;

public interface IExcelProductImporter
{
    Task<IReadOnlyList<ImportProductRowDto>> ParseAsync(Stream xlsxStream, CancellationToken cancellationToken);
}
