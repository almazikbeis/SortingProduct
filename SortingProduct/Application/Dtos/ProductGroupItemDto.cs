namespace SortingProduct.Application.Dtos;

public sealed record ProductGroupItemDto(
    Guid ProductBatchId,
    string Name,
    string Unit,
    decimal UnitPriceEur,
    int Quantity,
    decimal LineTotalEur
);
