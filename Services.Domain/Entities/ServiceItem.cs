namespace Services.Domain.Entities;

public partial class ServiceItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public Guid ProviderId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Propiedades nuevas para la geolocalización
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}