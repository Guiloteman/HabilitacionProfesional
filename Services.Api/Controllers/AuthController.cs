using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Domain.Entities;

namespace Services.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender<ApplicationUser> _emailSender;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender<ApplicationUser> emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(
            nameof(ConfirmEmail),
            "Auth",
            new { userId = user.Id, code = code },
            protocol: Request.Scheme);

        await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink ?? string.Empty);

        return Ok(new
        {
            Message = "Usuario registrado con éxito. Revisa tu correo para confirmar la cuenta."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized("Credenciales incorrectas o cuenta no confirmada.");
        }

        // Valida la contraseña y comprueba si el usuario tiene confirmado el correo (RequireConfirmedAccount = true)
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Inicio de sesión exitoso." });
        }

        if (result.IsNotAllowed)
        {
            return Unauthorized("Debes confirmar tu correo electrónico antes de iniciar sesión.");
        }

        return Unauthorized("Credenciales incorrectas.");
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return BadRequest("Parámetros de confirmación inválidos.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return Ok("¡Correo confirmado exitosamente! Ya puedes iniciar sesión.");
        }

        return BadRequest("Error al confirmar el correo electrónico.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Ok(new { Message = "Si el correo existe, se ha enviado un enlace." });
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = $"https://localhost:7094/reset-password?email={Uri.EscapeDataString(user.Email)}&code={Uri.EscapeDataString(code)}";

        await _emailSender.SendPasswordResetLinkAsync(user, user.Email, resetLink);

        return Ok(new { Message = "Correo de recuperación enviado." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Contraseña restablecida exitosamente.");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        // Redirige de vuelta a la página principal del frontend
        return Redirect("https://localhost:7094/");
    }

    [HttpGet("user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        // Verifica si el usuario actual está autenticado por cookies
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        // Busca al usuario en la base de datos usando su ID o correo actual
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.Identity.Name;
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return NotFound();
        }

        // Devuelve los datos necesarios (incluyendo su Nombre Completo)
        return Ok(new
        {
            email = user.Email,
            fullName = user.FullName // O la propiedad que almacene el nombre en tu ApplicationUser
        });
    }
}

public record RegisterDto(string Email, string Password, string FullName);
public record LoginDto(string Email, string Password);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Code, string NewPassword);