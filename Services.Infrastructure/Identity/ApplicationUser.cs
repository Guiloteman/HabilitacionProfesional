using Microsoft.AspNetCore.Identity;

namespace Services.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Aquí puedes agregar propiedades extras que no vienen por defecto en IdentityUser
        public string? FullName { get; set; }
    }
}