using KattiSSO.Models;
using KattiSSO.Models.Response;
using KattiSSO.Services.RefreshTokenRepostitory;
using KattiSSO.Services.TokenGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Services.Authenticators
{
    public class Authenticator
    {
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly AccessTokenGenerator _accessTokenGenerator;
        private readonly RefreshTokenGenerator _refreshTokenGenerator;

        public Authenticator(
            RefreshTokenRepository refreshTokenRepository,
            AccessTokenGenerator accessTokenGenerator,
            RefreshTokenGenerator refreshTokenGenerator)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        public async Task<AuthenticatedUserResponse> Authenticate(User user)
        {
            string accessToken = _accessTokenGenerator.GenerateToken(user);
            string refreshToken = _refreshTokenGenerator.GenerateToken();

            RefreshToken savedToken = new RefreshToken()
            {
                Token = refreshToken,
                user_id = user.user_id
            };

            await _refreshTokenRepository.Create(savedToken);

            return new AuthenticatedUserResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
