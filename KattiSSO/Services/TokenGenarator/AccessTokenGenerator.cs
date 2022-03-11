using cryptogram_backend.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace cryptogram_backend.Services.TokenGenarator
{
    public class AccessTokenGenerator
    {
        private readonly AuthenticationConfiguration _configuration;

        public AccessTokenGenerator(AuthenticationConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user)
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.AccessTokenSecret));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.username)
            };

            JwtSecurityToken token = new JwtSecurityToken(_configuration.Issuer,
                                                          _configuration.Audience,
                                                          claims,
                                                          DateTime.UtcNow,
                                                          DateTime.UtcNow.AddHours(_configuration.AccessTokenExpirationHours),
                                                          credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
