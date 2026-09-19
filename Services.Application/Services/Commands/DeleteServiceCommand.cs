using MediatR;

namespace Services.Application.Services.Commands;

public record DeleteServiceCommand(Guid Id) : IRequest<bool>;