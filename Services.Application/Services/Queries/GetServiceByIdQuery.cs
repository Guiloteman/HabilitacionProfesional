using MediatR;
using Services.Domain.Entities;

namespace Services.Application.Services.Queries;

public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceItem?>;