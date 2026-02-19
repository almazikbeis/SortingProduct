using Microsoft.AspNetCore.Mvc;
using SortingProduct.Application.Abstractions;
using SortingProduct.Application.Services;

namespace SortingProduct.Controllers;

/// <summary>
/// Utility endpoints for manual testing/demo.
/// </summary>
[ApiController]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly ProductGroupingService _groupingService;
    private readonly IProductBatchRepository _batchRepository;

    public AdminController(ProductGroupingService groupingService, IProductBatchRepository batchRepository)
    {
        _groupingService = groupingService;
        _batchRepository = batchRepository;
    }

    /// <summary>
    /// Manually runs grouping immediately (background job still exists).
    /// </summary>
    [HttpPost("grouping/run")]
    public async Task<ActionResult<object>> RunGrouping(CancellationToken cancellationToken)
    {
        var createdGroups = await _groupingService.CreateGroupsAsync(cancellationToken);
        return Ok(new { createdGroups });
    }

    /// <summary>
    /// View imported product batches and remaining quantities.
    /// </summary>
    [HttpGet("batches")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetBatches(CancellationToken cancellationToken)
    {
        var batches = await _batchRepository.GetAllAsync(cancellationToken);

        var dto = batches
            .Select(b => (object)new
            {
                b.Id,
                b.Name,
                b.Unit,
                b.UnitPriceEur,
                b.InitialQuantity,
                b.RemainingQuantity,
                b.Status,
                b.CreatedAt
            })
            .ToList();

        return Ok(dto);
    }
}
