namespace SortingProduct.Domain.Entities;

public sealed class ProductGroup
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal TotalPriceEur { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<ProductGroupItem> Items { get; set; } = new();
}
