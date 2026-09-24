using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Application.Services;
using Services.Application.Services.Commands;
using Services.Application.Services.Queries;
using Services.Domain.Entities;
using Services.Application.DTOs;


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
        var userIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var providerId))
        {
            return Unauthorized(new { message = "Usuario no autenticado." });
        }

        var getQuery = new GetServiceByIdQuery(id);
        var existingService = await _mediator.Send(getQuery);

        if (existingService == null)
            return NotFound(new { message = "El servicio que intentas eliminar no existe." });

        if (existingService.ProviderId != providerId)
        {
            return StatusCode(403, new { message = "No tienes permisos para eliminar este servicio porque no te pertenece." });
        }

        var command = new DeleteServiceCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "No se pudo eliminar el servicio." });

        return NoContent();
    }

    [HttpGet("providers")]
    public async Task<ActionResult<IEnumerable<ProviderDto>>> GetProviders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProvidersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/accept")]
    [Authorize]
    public async Task<IActionResult> AcceptService(Guid id)
    {
        var providerIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(providerIdString) || !Guid.TryParse(providerIdString, out var providerId))
        {
            return Unauthorized(new { message = "Prestador no identificado." });
        }

        var getQuery = new GetServiceByIdQuery(id);
        var existingService = await _mediator.Send(getQuery);

        if (existingService == null)
            return NotFound(new { message = "El servicio no existe." });

        var command = new UpdateServiceCommand(
            Id: id,
            Title: existingService.Title,
            Description: existingService.Description,
            Price: existingService.Price,
            ProviderId: providerId
        );

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "No se pudo asignar el servicio." });

        return NoContent();
    }

    [HttpGet("my-active-job")]
    [Authorize]
    public async Task<IActionResult> GetMyActiveJob()
    {
        var providerIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(providerIdString) || !Guid.TryParse(providerIdString, out var providerId))
        {
            return Unauthorized(new { message = "Prestador no autenticado." });
        }

        var query = new GetActiveJobByProviderQuery(providerId);
        var activeJob = await _mediator.Send(query);

        if (activeJob == null)
        {
            return NoContent();
        }

        return Ok(activeJob);
    }

    [HttpGet("my-services")]
    [Authorize]
    public async Task<IActionResult> GetMyServices()
    {
        var userIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var providerId))
        {
            return Unauthorized(new { message = "Prestador no autenticado." });
        }

        var query = new GetServicesQuery(SearchTerm: null, ProviderId: providerId, PageNumber: 1, PageSize: 50);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost("{id}/request")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestService(Guid id, [FromBody] ClientLocationDto dto)
    {
        var command = new UpdateClientLocationCommand(id, dto.Latitude, dto.Longitude);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "El servicio solicitado no existe." });
        }

        return Ok(new { message = "Solicitud enviada con éxito al prestador." });
    }

    [HttpPut("{id}/provider-location")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateProviderLocation(Guid id, [FromBody] ProviderLocationDto dto)
    {
        var command = new UpdateProviderLocationCommand(id, dto.ProviderLatitude, dto.ProviderLongitude);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "El servicio no existe o no se pudo actualizar la ubicación del prestador." });

        return NoContent();
    }
}