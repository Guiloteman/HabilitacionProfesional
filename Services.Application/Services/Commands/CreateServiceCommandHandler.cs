using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Domain.Entities;

namespace Services.Application.Services.Commands;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        // 1. Verificamos si el Provider ya existe en la base de datos
        var providerExists = await _context.Providers.AnyAsync(p => p.Id == request.ProviderId, cancellationToken);

        if (!providerExists)
        {
            // 2. Si no existe, creamos el proveedor directamente con el ID del usuario autenticado
            var newProvider = new Provider
            {
                Id = request.ProviderId,
                Name = "Tradesperson" // O puedes asignarle un valor predeterminado seguro
            };

            _context.Providers.Add(newProvider);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Procedemos a crear el servicio con normalidad
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            ProviderId = request.ProviderId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceItems.Add(serviceItem);
        await _context.SaveChangesAsync(cancellationToken);

        return serviceItem.Id;
    }
}