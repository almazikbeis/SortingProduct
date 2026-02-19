namespace SortingProduct.Application.Dtos;

public sealed record ProductGroupDto(
    Guid Id,
    string Name,
    decimal TotalPriceEur,
    DateTimeOffset CreatedAt
);
