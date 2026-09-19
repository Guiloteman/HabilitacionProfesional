using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Application.Common.Models;
using Services.Application.DTOs;

namespace Services.Application.Services.Queries;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, PagedResult<ServiceItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ServiceItemDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ServiceItems.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(s => s.Title.Contains(request.SearchTerm));
        }

        if (request.ProviderId.HasValue)
        {
            query = query.Where(s => s.ProviderId == request.ProviderId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Title)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ServiceItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Price = s.Price
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}