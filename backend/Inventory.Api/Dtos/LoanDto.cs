namespace Inventory.Api.Dtos;

public record LoanDto(
    int Id,
    int ItemId,
    string ItemModelBrand,
    string ItemTypeName,
    string? ItemPhotoUrl,
    DateTime CheckoutDate,
    DateTime? ReturnDueDate,
    string Technician,
    string BorrowedBy,
    DateTime? ReturnedAt,
    string RegisteredByName);
