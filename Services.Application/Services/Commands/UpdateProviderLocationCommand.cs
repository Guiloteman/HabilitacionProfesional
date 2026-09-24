using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;

namespace Services.Application.Services.Commands;

public record UpdateProviderLocationCommand(Guid ServiceId, decimal ProviderLatitude, decimal ProviderLongitude) : IRequest<bool>;

public class UpdateProviderLocationCommandHandler : IRequestHandler<UpdateProviderLocationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProviderLocationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProviderLocationCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.ServiceItems.FindAsync(new object[] { request.ServiceId }, cancellationToken);

        if (service == null)
            return false;

        service.ProviderLatitude = request.ProviderLatitude;
        service.ProviderLongitude = request.ProviderLongitude;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}