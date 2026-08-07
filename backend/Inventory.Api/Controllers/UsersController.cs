using System.Security.Claims;
using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Models;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(InventoryDbContext db) : ControllerBase
{
    // Protected admin account: cannot be removed nor have its name/password changed,
    // to guarantee there is always administrative access to the application.
    private const string ProtectedAdminUsername = "admin.besttechti";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var users = await db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username, u.FullName, u.IsAdmin, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username, full name, and password are required." });
        }

        var newUsername = request.Username.Trim();
        if (await db.Users.AnyAsync(u => u.Username == newUsername))
        {
            return BadRequest(new { message = "A user with that name already exists." });
        }

        var passwordError = PasswordPolicy.Validate(request.Password);
        if (passwordError is not null)
        {
            return BadRequest(new { message = passwordError });
        }

        var user = new User
        {
            Username = newUsername,
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsAdmin = request.IsAdmin,
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Safety net for the rare case of two concurrent requests passing the check
            // above at the same time: whoever hits the DB's unique index first wins, the
            // other gets this message instead of a 500 error.
            return BadRequest(new { message = "A user with that name already exists." });
        }

        return Ok(new UserDto(user.Id, user.Username, user.FullName, user.IsAdmin, user.CreatedAt));
    }

    [HttpPut("{id:int}")]
    [EnableRateLimiting("SensitiveActions")]
    public async Task<ActionResult<UserDto>> Update(int id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Username == ProtectedAdminUsername)
        {
            return BadRequest(new { message = "The default administrator account cannot be changed." });
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required." });
        }

        var newUsername = request.Username.Trim();
        if (await db.Users.AnyAsync(u => u.Id != id && u.Username == newUsername))
        {
            return BadRequest(new { message = "A user with that name already exists." });
        }

        user.Username = newUsername;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordError = PasswordPolicy.Validate(request.Password);
            if (passwordError is not null)
            {
                return BadRequest(new { message = passwordError });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "A user with that name already exists." });
        }

        return Ok(new UserDto(user.Id, user.Username, user.FullName, user.IsAdmin, user.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    [EnableRateLimiting("SensitiveActions")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (id == CurrentUserId())
        {
            return BadRequest(new { message = "You cannot remove your own user." });
        }

        if (user.Username == ProtectedAdminUsername)
        {
            return BadRequest(new { message = "The default administrator account cannot be removed." });
        }

        if (user.IsAdmin && await db.Users.CountAsync(u => u.IsAdmin) <= 1)
        {
            return BadRequest(new { message = "The only administrator account cannot be removed." });
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue("uid")!);
}
