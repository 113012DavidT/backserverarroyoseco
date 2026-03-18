using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using arroyoSeco.Application.Common.Interfaces;

namespace arroyoSeco.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;

    public StorageController(IStorageService storage)
    {
        _storage = storage;
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<ActionResult<string>> Upload([FromForm] IFormFile file, [FromQuery] string folder = "general", CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo vacío");

        using var stream = file.OpenReadStream();
        var relativePath = await _storage.SaveFileAsync(stream, file.FileName, folder, ct);
        var publicUrl = _storage.GetPublicUrl(relativePath);
        return Ok(new { url = publicUrl });
    }
}
