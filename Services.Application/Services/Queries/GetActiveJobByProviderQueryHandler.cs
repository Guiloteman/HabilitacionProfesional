using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Application.DTOs;

namespace Services.Application.Services.Queries;

public record GetActiveJobByProviderQuery(Guid ProviderId) : IRequest<ServiceItemDto?>;

public class GetActiveJobByProviderQueryHandler : IRequestHandler<GetActiveJobByProviderQuery, ServiceItemDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActiveJobByProviderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceItemDto?> Handle(GetActiveJobByProviderQuery request, CancellationToken cancellationToken)
    {
        var activeJob = await _context.ServiceItems
            .Where(s => s.ProviderId == request.ProviderId && s.Latitude != null)
            .Select(s => new ServiceItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Price = s.Price
            })
            .FirstOrDefaultAsync(cancellationToken);

        return activeJob;
    }
}