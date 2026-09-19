using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;

public record GetProvidersQuery() : IRequest<IEnumerable<ProviderDto>>;

public record ProviderDto(Guid Id, string Name);

public class GetProvidersQueryHandler : IRequestHandler<GetProvidersQuery, IEnumerable<ProviderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProvidersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ProviderDto>> Handle(GetProvidersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Providers
            .Select(p => new ProviderDto(p.Id, p.Name))
            .ToListAsync(cancellationToken);
    }
}