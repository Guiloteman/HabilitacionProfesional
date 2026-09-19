using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Services.Application.Services.Commands;
using Services.Infrastructure.Persistence.Scaffolded;

namespace Services.Application.UnitTests.Services.Commands;

public class CreateServiceCommandHandlerTests
{
    private readonly ServicesDbContext _context;

    public CreateServiceCommandHandlerTests()
    {
        // 1. Configuramos una base de datos en memoria única para aislar la prueba
        var options = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ServicesDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateServiceItemAndReturnId_WhenCommandIsValid()
    {
        // Arrange (Preparación de datos y comando)
        var handler = new CreateServiceCommandHandler(_context);

        var command = new CreateServiceCommand(
            Title: "Servicio de Electricidad",
            Description: "Instalación de tableros eléctricos",
            Price: 120.0m,
            ProviderId: Guid.NewGuid()
        );

        var serviceId = await handler.Handle(command, CancellationToken.None);

        serviceId.Should().NotBeEmpty();

        var createdService = await _context.ServiceItems.FindAsync(serviceId);

        createdService.Should().NotBeNull();
        createdService!.Title.Should().Be("Servicio de Electricidad");
        createdService.Price.Should().Be(120.0m);
        createdService.ProviderId.Should().Be(command.ProviderId);
    }
}