namespace Services.Application.DTOs;

public class ServiceItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Coordenadas de ubicación del cliente
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}