namespace Services.Application.DTOs;

public class ServiceItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? ProviderLatitude { get; set; }
    public decimal? ProviderLongitude { get; set; }
}