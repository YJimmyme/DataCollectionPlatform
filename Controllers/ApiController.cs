using DataCollectionPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataCollectionPlatform.Controllers;

[Route("api")]
public class ApiController(IUrlFetchService urlFetchService) : Controller
{
    [HttpPost("fetch-url")]
    public async Task<IActionResult> FetchUrl([FromBody] FetchUrlRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.Url))
            return BadRequest(new { error = "URL 不可為空" });

        var metadata = await urlFetchService.FetchAsync(req.Url);
        if (metadata == null)
            return Ok(new { title = (string?)null, description = (string?)null, author = (string?)null });

        return Ok(new
        {
            title = metadata.Title,
            description = metadata.Description,
            author = metadata.Author
        });
    }
}

public record FetchUrlRequest(string? Url);
