        using MediatR;
        using Microsoft.EntityFrameworkCore;
        using Services.Application.Common.Interfaces;

        namespace Services.Application.Services.Commands;

        public record UpdateClientLocationCommand(Guid ServiceId, decimal Latitude, decimal Longitude) : IRequest<bool>;

        public class UpdateClientLocationCommandHandler : IRequestHandler<UpdateClientLocationCommand, bool>
        {
            private readonly IApplicationDbContext _context;

            public UpdateClientLocationCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<bool> Handle(UpdateClientLocationCommand request, CancellationToken cancellationToken)
            {
                var service = await _context.ServiceItems.FindAsync(new object[] { request.ServiceId }, cancellationToken);

                if (service == null)
                    return false;

                // Actualizamos la ubicación del cliente. 
                // El hecho de que Latitude y Longitude tengan valores ya indica que fue solicitado.
                service.Latitude = request.Latitude;
                service.Longitude = request.Longitude;

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }