namespace Services.Domain.Entities;

public class Provider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Relación opcional: un proveedor puede tener muchos servicios
    public ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
}