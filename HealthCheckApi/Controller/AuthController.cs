using HealthCheckApi.Dto;
using HealthCheckApi.Enums;
using HealthCheckApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckApi.Controller;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.Login(request, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.Unauthorized)
            return Unauthorized();

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshToken(request, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.Unauthorized)
            return Unauthorized();

        if (error.Type == TypeErrors.BusinessRule)
            return BadRequest(error);

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }
}
