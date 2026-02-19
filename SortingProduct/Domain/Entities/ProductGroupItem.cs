namespace SortingProduct.Domain.Entities;

public sealed class ProductGroupItem
{
    public Guid Id { get; set; }

    public Guid ProductGroupId { get; set; }

    public Guid ProductBatchId { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotalEur { get; set; }

    public ProductGroup? ProductGroup { get; set; }

    public ProductBatch? ProductBatch { get; set; }
}
