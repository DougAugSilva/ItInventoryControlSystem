namespace Inventory.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string Username, string FullName, bool IsAdmin);
