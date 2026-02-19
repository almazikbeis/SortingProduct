using SortingProduct.Application.Abstractions;
using SortingProduct.Domain.Entities;

namespace SortingProduct.Application.Services;

public sealed class ProductGroupingService
{
    public const decimal GroupLimitEur = 200m;

    private readonly IProductBatchRepository _productBatchRepository;
    private readonly IProductGroupRepository _productGroupRepository;

    public ProductGroupingService(IProductBatchRepository productBatchRepository, IProductGroupRepository productGroupRepository)
    {
        _productBatchRepository = productBatchRepository;
        _productGroupRepository = productGroupRepository;
    }

    public async Task<int> CreateGroupsAsync(CancellationToken cancellationToken)
    {
        var availableBatches = await _productBatchRepository.GetAvailableForGroupingAsync(cancellationToken);
        if (availableBatches.Count == 0)
        {
            return 0;
        }

        // Идея алгоритма (простыми словами):
        // Мы собираем группу до 200 евро. Чтобы сумма получалась ближе к 200,
        // каждый раз берём "самый дорогой товар", который ещё помещается в остаток.
        // Это не идеальная математика (не knapsack), но на практике даёт группы почти под лимит.
        // Количество можно делить: часть партии ушла в группу 1, остаток - в следующие.
        var remainingBatches = availableBatches
            .Where(batch => batch.RemainingQuantity > 0)
            .ToList();

        var createdGroups = 0;

        while (HasAnyRemainingQuantity(remainingBatches))
        {
            var productGroup = CreateNewGroup(createdGroups + 1);
            FillGroup(productGroup, remainingBatches);

            // Если вообще ничего не смогли положить, значит дальше уже не соберём.
            // (обычно это бывает если цены <= 0 или остаток группы меньше минимальной цены)
            if (productGroup.Items.Count == 0)
            {
                MarkImpossibleBatchesAsProcessed(remainingBatches);
                break;
            }

            await _productGroupRepository.AddAsync(productGroup, cancellationToken);
            await _productGroupRepository.SaveChangesAsync(cancellationToken);
            await _productBatchRepository.SaveChangesAsync(cancellationToken);

            createdGroups++;
        }

        return createdGroups;
    }

    private static bool HasAnyRemainingQuantity(List<ProductBatch> batches) =>
        batches.Any(batch => batch.RemainingQuantity > 0);

    private static ProductGroup CreateNewGroup(int groupNumber) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = $"Group {groupNumber}",
            CreatedAt = DateTimeOffset.UtcNow,
            TotalPriceEur = 0m,
            Items = new List<ProductGroupItem>()
        };

    private static void FillGroup(ProductGroup group, List<ProductBatch> remainingBatches)
    {
        var remainingGroupCapacityEur = GroupLimitEur;

        while (remainingGroupCapacityEur > 0.0001m)
        {
            var selectedBatch = SelectBestBatchThatFits(remainingBatches, remainingGroupCapacityEur);
            if (selectedBatch is null)
            {
                break;
            }

            var maxUnitsThatFit = CalculateMaxUnitsThatFit(remainingGroupCapacityEur, selectedBatch.UnitPriceEur);
            if (maxUnitsThatFit <= 0)
            {
                break;
            }

            var quantityToTake = Math.Min(selectedBatch.RemainingQuantity, maxUnitsThatFit);
            if (quantityToTake <= 0)
            {
                break;
            }

            var lineTotalEur = quantityToTake * selectedBatch.UnitPriceEur;

            group.Items.Add(new ProductGroupItem
            {
                Id = Guid.NewGuid(),
                ProductGroupId = group.Id,
                ProductBatchId = selectedBatch.Id,
                Quantity = quantityToTake,
                LineTotalEur = lineTotalEur
            });

            group.TotalPriceEur += lineTotalEur;
            remainingGroupCapacityEur -= lineTotalEur;

            selectedBatch.RemainingQuantity -= quantityToTake;
            selectedBatch.Status = selectedBatch.RemainingQuantity == 0
                ? ProductBatchStatus.Processed
                : ProductBatchStatus.Processing;
        }
    }

    // Суть "подбора ближе к 200":
    // выбираем максимально дорогую позицию, которая помещается в оставшееся место.
    private static ProductBatch? SelectBestBatchThatFits(List<ProductBatch> batches, decimal remainingGroupCapacityEur) =>
        batches
            .Where(batch => batch.RemainingQuantity > 0 && batch.UnitPriceEur > 0m && batch.UnitPriceEur <= remainingGroupCapacityEur)
            .OrderByDescending(batch => batch.UnitPriceEur)
            .FirstOrDefault();

    private static int CalculateMaxUnitsThatFit(decimal remainingGroupCapacityEur, decimal unitPriceEur)
    {
        if (unitPriceEur <= 0m)
        {
            return 0;
        }

        return (int)decimal.Floor(remainingGroupCapacityEur / unitPriceEur);
    }

    private static void MarkImpossibleBatchesAsProcessed(List<ProductBatch> batches)
    {
        foreach (var batch in batches.Where(b => b.UnitPriceEur <= 0m))
        {
            batch.RemainingQuantity = 0;
            batch.Status = ProductBatchStatus.Processed;
        }
    }
}
