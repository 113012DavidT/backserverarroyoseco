using Microsoft.AspNetCore.Identity;

namespace arroyoSeco.Domain.Entities.Usuarios;

public class ApplicationUser : IdentityUser
{
    public string Direccion { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;
    public bool RequiereCambioPassword { get; set; }
    public DateTime? FechaPrimerLogin { get; set; }

    public bool PerfilBasicoCompleto =>
        !string.IsNullOrWhiteSpace(Direccion) &&
        !string.IsNullOrWhiteSpace(Sexo);
}
