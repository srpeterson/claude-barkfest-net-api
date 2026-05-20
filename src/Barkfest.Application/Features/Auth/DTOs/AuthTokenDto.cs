namespace Barkfest.Application.Features.Auth.DTOs;

public record AuthTokenDto(string AccessToken, Guid AccountId, DateTime ExpiresAt);
