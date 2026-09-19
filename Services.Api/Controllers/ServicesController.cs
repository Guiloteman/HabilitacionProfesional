using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Application.Services;
using Services.Application.Services.Commands;
using Services.Application.Services.Queries;
using Services.Domain.Entities;

namespace Services.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public ServicesController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? providerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetServicesQuery(searchTerm, providerId, pageNumber, pageSize);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR EN MEDIATR / EF CORE: {ex.Message} ===");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceCommand command)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuario no autenticado." });
        }
        var updatedCommand = command with { ProviderId = Guid.Parse(userId) };

        var serviceId = await _mediator.Send(updatedCommand);
        return CreatedAtAction(nameof(GetById), new { id = serviceId }, serviceId);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetServiceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "El servicio solicitado no existe." });

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { message = "El ID de la ruta no coincide con el del cuerpo de la petición." });

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "El servicio que intentas actualizar no existe." });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteServiceCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "El servicio que intentas eliminar no existe." });

        return NoContent();
    }

    [HttpGet("providers")]
    public async Task<ActionResult<IEnumerable<ProviderDto>>> GetProviders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProvidersQuery(), cancellationToken);
        return Ok(result);
    }
}