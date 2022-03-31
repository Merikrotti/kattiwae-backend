using KattiSSO.Models;
using KattiSSO.Services.UserRepositories;
using System.Collections.Generic;
using System.Security.Claims;

namespace KattiSSO.Services.TokenGenerator
{
    public class AccessTokenGenerator
    {
        private readonly AuthenticationConfiguration _configuration;
        private readonly TokenGenerator _tokenGenerator;
        private readonly RoleRepository _roleRepository;

        public AccessTokenGenerator(AuthenticationConfiguration configuration, TokenGenerator tokenGenerator, RoleRepository roleRepository)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _roleRepository = roleRepository;
        }
        public string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.username)
            };
            var roles = _roleRepository.GetByUser(user.user_id);
            if(roles != null)
                foreach (var role in roles)
                    claims.Add(role);

            return _tokenGenerator.GenerateToken(
                _configuration.AccessTokenSecret,
                _configuration.Issuer,
                _configuration.Audience,
                _configuration.AccessTokenExpirationHours,
                claims);
        }
    }
}
