namespace Barkfest.Application.Features.Administrators.DTOs;

public record AdministratorDto(
    Guid Id,
    string Username,
    string Name,
    string Email,
    string PhoneNumber,
    DateTime CreatedAt);
