using Book.v2.Data;
using Book.v2.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ContextDb _context;

    public AuthController(ContextDb context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null)
        {
            return BadRequest(new { message = "E-posta veya şifre hatalı." });
        }

        return Ok(new { id = user.Id, username = user.Username, email = user.Email });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
        }

        var user = Models.Entities.User.Create(request.Username, request.Email);
        _context.Users.Add(user);
        
        var defaultPreferences = UserPreference.Create(user.Id, new[] { "Kurgu" }, new[] { "yeni" });
        _context.Set<UserPreference>().Add(defaultPreferences);

        await _context.SaveChangesAsync();

        return Ok(new { id = user.Id, username = user.Username, email = user.Email });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
