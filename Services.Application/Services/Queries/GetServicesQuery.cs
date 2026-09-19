using MediatR;
using Services.Application.Common.Models;
using Services.Application.DTOs;

namespace Services.Application.Services.Queries;

public record GetServicesQuery(
    string? SearchTerm,
    Guid? ProviderId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<ServiceItemDto>>;