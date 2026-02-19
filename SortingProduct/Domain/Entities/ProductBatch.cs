namespace SortingProduct.Domain.Entities;

public sealed class ProductBatch
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal UnitPriceEur { get; set; }

    public int InitialQuantity { get; set; }

    public int RemainingQuantity { get; set; }

    public ProductBatchStatus Status { get; set; } = ProductBatchStatus.New;

    public DateTimeOffset CreatedAt { get; set; }
}
