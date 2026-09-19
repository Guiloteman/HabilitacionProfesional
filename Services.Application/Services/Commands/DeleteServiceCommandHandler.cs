using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;

namespace Services.Application.Services.Commands;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var serviceItem = await _context.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (serviceItem == null)
            return false;

        // Eliminación física del registro
        _context.ServiceItems.Remove(serviceItem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}