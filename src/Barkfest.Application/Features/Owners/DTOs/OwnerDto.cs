using Barkfest.Application.Features.Pets.DTOs;

namespace Barkfest.Application.Features.Owners.DTOs;

public record OwnerDto(
    Guid Id,
    string Username,
    string? DisplayName,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsVisible,
    ProfileImageDto? ProfileImage,
    DateTime CreatedAt);
