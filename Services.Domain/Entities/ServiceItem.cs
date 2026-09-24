namespace Services.Domain.Entities;

public partial class ServiceItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public Guid ProviderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? ProviderLatitude { get; set; }

    public decimal? ProviderLongitude { get; set; }
}