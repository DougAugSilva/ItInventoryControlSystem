namespace Inventory.Api.Dtos;

public record UserDto(int Id, string Username, string FullName, bool IsAdmin, DateTime CreatedAt);

public record CreateUserRequest(string Username, string FullName, string Password, bool IsAdmin);

public record UpdateUserRequest(string Username, string? Password);
