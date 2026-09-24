using Microsoft.AspNetCore.Identity;

namespace Services.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}