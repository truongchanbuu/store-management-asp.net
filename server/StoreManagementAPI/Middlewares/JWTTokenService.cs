using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Models;
using StoreManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StoreManagementAPI.Middlewares
{
    public class JWTTokenService
    {
        private readonly string _secret;
        private readonly int _seconds;

        public JWTTokenService(IOptions<JWTSettings> settings)
        {
            _secret = settings.Value.Secret;
            _seconds = settings.Value.ExpireInSecond;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            var claims = new List<Claim>();

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Email));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "self",
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(_seconds),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                if (jwtToken.Claims.Any())
                    return jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
