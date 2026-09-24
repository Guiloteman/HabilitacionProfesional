using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;

namespace Services.Application.Services.Commands;

public record AcceptServiceCommand(
    Guid ServiceId,
    decimal ProviderLatitude,
    decimal ProviderLongitude
) : IRequest<bool>;

public class AcceptServiceCommandHandler : IRequestHandler<AcceptServiceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AcceptServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AcceptServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service == null)
            return false;

        // Actualizamos las coordenadas del prestador al aceptar el viaje
        service.ProviderLatitude = request.ProviderLatitude;
        service.ProviderLongitude = request.ProviderLongitude;

        // Si manejas un campo de estado (ej. Status), puedes cambiarlo aquí:
        // service.Status = "InProcess"; 

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}