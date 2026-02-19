using Microsoft.AspNetCore.Mvc;
using SortingProduct.Application.Services;

namespace SortingProduct.Controllers;

[ApiController]
[Route("api/import")]
public sealed class ImportController : ControllerBase
{
    private readonly ProductImportService _importService;

    public ImportController(ProductImportService importService)
    {
        _importService = importService;
    }

    public sealed class ImportXlsxRequest
    {
        public IFormFile? File { get; set; }
    }

    [HttpPost("xlsx")]
    [RequestSizeLimit(50_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> ImportXlsx([FromForm] ImportXlsxRequest request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .xlsx is supported.");
        }

        await using var stream = file.OpenReadStream();
        var importedCount = await _importService.ImportAsync(stream, cancellationToken);

        return Ok(new { importedCount });
    }
}
