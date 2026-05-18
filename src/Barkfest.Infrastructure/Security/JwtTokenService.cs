using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Barkfest.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public string GenerateOwnerToken(Owner owner)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, owner.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, owner.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, owner.FirstName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim("account_type", "owner")
        };

        return BuildToken(claims);
    }

    public string GenerateAdminToken(Administrator administrator)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, administrator.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, administrator.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim("account_type", "admin")
        };

        return BuildToken(claims);
    }

    public DateTime GetExpiry() =>
        DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

    private string BuildToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: GetExpiry(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
