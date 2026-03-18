using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Usuarios;

namespace arroyoSeco.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _token;
    private readonly IAppDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator token,
        IAppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _token = token;
        _db = db;
    }

    public record RegisterDto(string Email, string Password, string? Role, int? TipoOferente);
    public record LoginDto(string Email, string Password);
    public record CambiarPasswordDto(string PasswordActual, string PasswordNueva);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // Asignar por defecto rol Cliente (antes era Oferente)
        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Cliente" : dto.Role!;
        await _userManager.AddToRoleAsync(user, role);

        // Crear entidad Oferente si el rol es Oferente
        if (role == "Oferente")
        {
            var tipoOferente = dto.TipoOferente.HasValue 
                ? (Domain.Entities.Enums.TipoOferente)dto.TipoOferente.Value 
                : Domain.Entities.Enums.TipoOferente.Ambos;

            var oferente = new Oferente
            {
                Id = user.Id,
                Nombre = dto.Email.Split('@')[0],
                NumeroAlojamientos = 0,
                Tipo = tipoOferente
            };
            _db.Oferentes.Add(oferente);
            await _db.SaveChangesAsync();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var jwt = _token.Generate(user.Id, user.Email!, roles);
        return Ok(new { token = jwt });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded) return Unauthorized();

        // Actualizar FechaPrimerLogin si es la primera vez
        if (!user.FechaPrimerLogin.HasValue)
        {
            user.FechaPrimerLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var jwt = _token.Generate(user.Id, user.Email!, roles, user.RequiereCambioPassword);
        return Ok(new { token = jwt });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { id = user.Id, email = user.Email, roles });
    }

    [Authorize]
    [HttpPost("cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, dto.PasswordActual, dto.PasswordNueva);
        if (!result.Succeeded) 
            return BadRequest(new { message = "Contraseña actual incorrecta o la nueva contraseña no cumple con los requisitos", errors = result.Errors });

        // Marcar que ya cambió la contraseña
        if (user.RequiereCambioPassword)
        {
            user.RequiereCambioPassword = false;
            await _userManager.UpdateAsync(user);
        }

        return Ok(new { message = "Contraseña actualizada exitosamente" });
    }
}