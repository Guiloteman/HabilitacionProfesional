using MediatR;

namespace Services.Application.Services.Commands;

public record CreateServiceCommand(
    string Title,
    string Description,
    decimal Price,
    Guid ProviderId
) : IRequest<Guid>;