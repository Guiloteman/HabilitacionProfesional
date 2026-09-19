using MediatR;

namespace Services.Application.Services.Commands;

public record UpdateServiceCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    Guid ProviderId
) : IRequest<bool>;