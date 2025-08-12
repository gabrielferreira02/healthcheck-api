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
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.CreateUser(request, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.Validation && error.Error is ValidationProblemDetails details)
            return ValidationProblem(details);

        if (error.Type == TypeErrors.BusinessRule)
            return BadRequest(error);

        return StatusCode(500, error);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken ct)
    {
        await _userService.DeleteUser(id, ct);
        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "USER")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _userService.GetUserById(id, ct);

        if (result.IsT0)
            return Ok(result.AsT0);

        var error = result.AsT1;

        if (error.Type == TypeErrors.NotFound)
            return NotFound();

        return StatusCode(500, error);
    }
}
