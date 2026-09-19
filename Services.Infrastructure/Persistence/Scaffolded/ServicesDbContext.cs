using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- 1. Nuevo using
using Microsoft.AspNetCore.Identity;                     // <-- 2. Nuevo using
using Services.Domain.Entities;
using Services.Application.Common.Interfaces;

namespace Services.Infrastructure.Persistence.Scaffolded;

// 3. Heredamos de IdentityDbContext (asumiendo que crearás una clase ApplicationUser)
public partial class ServicesDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ServicesDbContext()
    {
    }

    public ServicesDbContext(DbContextOptions<ServicesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ServiceItem> ServiceItems { get; set; }
    public virtual DbSet<Provider> Providers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ServiceMarketplaceDb;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 4. ¡CRÍTICO! Siempre debes llamar a la clase base primero cuando usas IdentityDbContext
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceI__3214EC07AE002468");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}