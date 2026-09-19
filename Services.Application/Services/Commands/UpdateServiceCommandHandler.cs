using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;

namespace Services.Application.Services.Commands;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ServiceItems.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.ProviderId = request.ProviderId; // <-- Asegúrate de actualizar esta propiedad

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}