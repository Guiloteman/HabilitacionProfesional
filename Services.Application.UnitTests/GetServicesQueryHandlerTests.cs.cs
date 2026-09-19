using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Services.Application.Services.Queries;
using Services.Domain.Entities;
using Services.Infrastructure.Persistence.Scaffolded;
using AutoMapper;

namespace Services.Application.UnitTests.Services.Queries;

public class GetServicesQueryHandlerTests
{
    private readonly ServicesDbContext _context;
    private readonly Mock<IMapper> _mapperMock;

    public GetServicesQueryHandlerTests()
    {
        // 1. Configuramos la base de datos en memoria
        var options = new DbContextOptionsBuilder<ServicesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ServicesDbContext(options);

        // 2. Creamos el Mock de IMapper para evitar problemas de configuración
        _mapperMock = new Mock<IMapper>();

        _mapperMock
            .Setup(m => m.Map<IEnumerable<DTOs.ServiceItemDto>>(It.IsAny<IEnumerable<ServiceItem>>()))
            .Returns((IEnumerable<ServiceItem> source) => source.Select(s => new DTOs.ServiceItemDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Price = s.Price,
                ProviderId = s.ProviderId,
                CreatedAt = s.CreatedAt
            }));
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedListOfServiceDtos_WhenServicesExist()
    {
        // Arrange
        _context.ServiceItems.Add(new ServiceItem
        {
            Id = Guid.NewGuid(),
            Title = "Servicio de Plomería",
            Description = "Reparación de tuberías",
            Price = 50.0m,
            ProviderId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        var handler = new GetServicesQueryHandler(_context);
        var query = new GetServicesQuery(SearchTerm: null, ProviderId: null, PageNumber: 1, PageSize: 10);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Title.Should().Be("Servicio de Plomería");
    }
}