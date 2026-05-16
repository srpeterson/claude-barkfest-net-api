using Barkfest.Application.Features.Pets.DTOs;

namespace Barkfest.Application.Features.Owners.DTOs;

public record OwnerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    ProfileImageDto? ProfileImage,
    DateTime CreatedAt);
