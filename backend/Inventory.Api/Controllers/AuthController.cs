using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(InventoryDbContext db, JwtService jwtService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = jwtService.GenerateToken(user);
        return Ok(new LoginResponse(token, user.Username, user.FullName, user.IsAdmin));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        var fullName = User.FindFirstValue(JwtRegisteredClaimNames.Name);
        var username = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole("Admin");
        return Ok(new { username, fullName, isAdmin });
    }
}
