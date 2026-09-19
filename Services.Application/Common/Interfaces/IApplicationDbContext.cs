using Microsoft.EntityFrameworkCore;
using Services.Domain.Entities;

namespace Services.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ServiceItem> ServiceItems { get; }
    DbSet<Provider> Providers { get; } // <-- Añadido

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}