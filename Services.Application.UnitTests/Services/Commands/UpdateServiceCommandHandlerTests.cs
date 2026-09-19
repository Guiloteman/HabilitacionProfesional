using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Services.Application.Services.Commands;
using Services.Domain.Entities;
using Services.Infrastructure.Persistence.Scaffolded;

namespace Services.Application.UnitTests.Services.Commands;

public class UpdateServiceCommandHandlerTests
{
    private readonly ServicesDbContext _context;

    public UpdateServiceCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ServicesDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldUpdateServiceItemAndReturnTrue_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var providerId = Guid.NewGuid(); // Definimos un ProviderId de prueba

        var existingService = new ServiceItem
        {
            Id = serviceId,
            Title = "Título Antiguo",
            Description = "Descripción antigua",
            Price = 30.0m,
            ProviderId = providerId
        };

        _context.ServiceItems.Add(existingService);
        await _context.SaveChangesAsync();

        var handler = new UpdateServiceCommandHandler(_context);

        // Agregamos ProviderId para que coincida con la definición del record/comando
        var command = new UpdateServiceCommand(
            Id: serviceId,
            Title: "Título Actualizado",
            Description: "Descripción actualizada",
            Price: 75.0m,
            ProviderId: providerId
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();

        var updatedService = await _context.ServiceItems.FindAsync(serviceId);

        updatedService.Should().NotBeNull();
        updatedService!.Title.Should().Be("Título Actualizado");
        updatedService.Description.Should().Be("Descripción actualizada");
        updatedService.Price.Should().Be(75.0m);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenServiceDoesNotExist()
    {
        var handler = new UpdateServiceCommandHandler(_context);

        // Agregamos un ProviderId aleatorio aquí también
        var command = new UpdateServiceCommand(
            Id: Guid.NewGuid(),
            Title: "No importa",
            Description: "No importa",
            Price: 10.0m,
            ProviderId: Guid.NewGuid()
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }
}