using System.ComponentModel;
using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Errors;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckApi.Controller;

[ApiController]
[Route("[controller]")]
public class UrlController : ControllerBase
{

    private readonly IUrlService _urlService;

    public UrlController(IUrlService urlService)
    {
        _urlService = urlService;
    }

    [HttpPost]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(typeof(UrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUrl([FromBody] CreateUrlRequest request, CancellationToken ct)
    {
        var result = await _urlService.CreateUrl(request, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.Validation && error.Error is ValidationProblemDetails details)
            return ValidationProblem(details);

        if (error.Type == TypeErrors.BusinessRule)
            return BadRequest(error);

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }

    [HttpPut]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(typeof(UrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUrl([FromBody] UpdateUrlRequest request, CancellationToken ct)
    {
        var result = await _urlService.UpdateUrlById(request, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.Validation && error.Error is ValidationProblemDetails details)
            return ValidationProblem(details);

        if (error.Type == TypeErrors.BusinessRule)
            return BadRequest(error);

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUrl([FromRoute] Guid id, CancellationToken ct)
    {
        await _urlService.DeleteUrl(id, ct);
        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(typeof(UrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUrlById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _urlService.GetUrlById(id, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }

    [HttpGet("user/{id}")]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(typeof(List<UrlResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserUrls([FromRoute] Guid id, CancellationToken ct)
    {
        var urls = await _urlService.GetUrlsByUserId(id, ct);
        return Ok(urls);
    }

}
