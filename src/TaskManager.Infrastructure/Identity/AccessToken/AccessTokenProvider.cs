using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Identity.AccessToken;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly IConfiguration _configuration;

    public AccessTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(TaskManagerUser user)
    {
        var secretKey = _configuration["Jwt:Ley"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Nickname, user.UserName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = _configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpirationInMinutes")),
            Issuer = _configuration["Jwt:Issuer"],
            IssuedAt = null,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(claims)
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}