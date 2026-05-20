using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Barkfest.Tests.Common.Helpers;

public static class JwtTestHelper
{
    private const string TestSecretKey = "barkfest-test-secret-key-min-32-chars!";
    private const string Issuer = "barkfest-api";
    private const string Audience = "barkfest-client";

    public static string GenerateOwnerToken(Guid ownerId, string email = "test@example.com")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, ownerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("account_type", "owner")
        };

        return BuildToken(claims);
    }

    public static string GenerateAdminToken(Guid adminId, string email = "admin@barkfest.dev")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, adminId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("account_type", "admin")
        };

        return BuildToken(claims);
    }

    private static string BuildToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
