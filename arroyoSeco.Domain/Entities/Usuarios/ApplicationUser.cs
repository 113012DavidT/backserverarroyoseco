using Microsoft.AspNetCore.Identity;

namespace arroyoSeco.Domain.Entities.Usuarios;

public class ApplicationUser : IdentityUser
{
    public bool RequiereCambioPassword { get; set; }
    public DateTime? FechaPrimerLogin { get; set; }
}
