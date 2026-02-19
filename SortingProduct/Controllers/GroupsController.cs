using Microsoft.AspNetCore.Mvc;
using SortingProduct.Application.Abstractions;
using SortingProduct.Application.Dtos;

namespace SortingProduct.Controllers;

[ApiController]
[Route("api/groups")]
public sealed class GroupsController : ControllerBase
{
    private readonly IProductGroupRepository _groupRepository;

    public GroupsController(IProductGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductGroupDto>>> GetGroups([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);
        skip = Math.Max(skip, 0);

        var groups = await _groupRepository.GetGroupsAsync(skip, take, cancellationToken);

        var groupDtos = groups.Select(g => new ProductGroupDto(g.Id, g.Name, g.TotalPriceEur, g.CreatedAt)).ToList();
        return Ok(groupDtos);
    }

    [HttpGet("{groupId:guid}/items")]
    public async Task<ActionResult<IReadOnlyList<ProductGroupItemDto>>> GetGroupItems([FromRoute] Guid groupId, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupWithItemsAsync(groupId, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        var items = group.Items.Select(i => new ProductGroupItemDto(
            i.ProductBatchId,
            i.ProductBatch?.Name ?? string.Empty,
            i.ProductBatch?.Unit ?? string.Empty,
            i.ProductBatch?.UnitPriceEur ?? 0m,
            i.Quantity,
            i.LineTotalEur
        )).ToList();

        return Ok(items);
    }
}
