namespace SortingProduct.Application.Dtos;

public sealed record ImportProductRowDto(
    string Name,
    string Unit,
    decimal UnitPriceEur,
    int Quantity
);
