using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Domain.Entities;

namespace Services.Application.Services.Queries;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceItem?>
{
    private readonly IApplicationDbContext _context;

    public GetServiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceItem?> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        // Busca el servicio por su Id de forma asíncrona
        return await _context.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
    }
}