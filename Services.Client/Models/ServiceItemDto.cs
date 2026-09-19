namespace Services.Client.Models
{
    public class ServiceItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid ProviderId { get; set; }
    }
}
