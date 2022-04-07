using KattiSSO.Models;
using KattiSSO.Models.Request;
using KattiSSO.Models.Response;
using KattiSSO.Services.Authenticators;
using KattiSSO.Services.PasswordHasher;
using KattiSSO.Services.RefreshTokenRepostitory;
using KattiSSO.Services.TokenGenerator;
using KattiSSO.Services.TokenValidators;
using KattiSSO.Services.UserRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KattiSSO.Controllers
{
    /// <summary>
    /// JWT Authentication controller
    /// 
    /// Created with help of
    /// https://www.youtube.com/watch?v=vNaDR2fPLKU
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("Service")]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly RefreshTokenValidator _refreshTokenValidator;
        private readonly Authenticator _authenticator;
        private readonly RoleRepository _roleRepository;

        public AuthController(
            IUserRepository userRepository,
            IPasswordHasher pwhash,
            RefreshTokenRepository refreshTokenRepository,
            AccessTokenGenerator tokenGenerator,
            RefreshTokenGenerator refreshTokenGenerator,
            RefreshTokenValidator refreshTokenValidator,
            Authenticator authenticator,
            RoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = pwhash;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenValidator = refreshTokenValidator;
            _authenticator = authenticator;
            _roleRepository = roleRepository;
        }

        [Authorize]
        [HttpGet("GetAccountData")]
        public IActionResult GetAccountData()
        {
            string id = HttpContext.User.FindFirstValue("id");
            string name = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            var roleclaims = HttpContext.User.FindAll(ClaimTypes.Role);
            List<string> roles = new List<string>();
            foreach (var item in roleclaims)
                roles.Add(item.ToString());
            return Ok(new { id = id, name = name, roles = roles});
        }

        [Authorize]
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleRepository.GetAllRoles();
            return Ok(new { roles = roles });
        }

        [Authorize]
        [HttpPost("linking")]
        public async Task<IActionResult> RoleLinking([FromBody] RoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string raw_id = HttpContext.User.FindFirstValue("id");

            if (!int.TryParse(raw_id, out int user_id))
            {
                return Unauthorized();
            }

            bool success = await _roleRepository.AddRole(user_id, request.SecretToken);
            if (!success)
                return BadRequest();
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if(model.password != model.confirmpassword)
            {
                return BadRequest();
            }

            User existingUser = await _userRepository.GetByName(model.username);
            if (existingUser != null)
                return Conflict();

            string pwHash = _passwordHasher.HashPassword(model.password);

            User registerationUser = new User()
            {
                username = model.username,
                password = pwHash
            };

            await _userRepository.Create(registerationUser);

            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            bool isValidToken = _refreshTokenValidator.Validate(refRequest.RefreshToken);

            if (!isValidToken)
                return BadRequest(new { error = "Invalid refresh token." });

            RefreshToken refreshTokenFound = await _refreshTokenRepository.GetByToken(refRequest.RefreshToken);
            if (refreshTokenFound == null)
                return BadRequest(new { error = "Invalid refresh token." });

            await _refreshTokenRepository.Delete(refreshTokenFound.ref_id);

            User user = await _userRepository.GetById(refreshTokenFound.user_id);
            if(user == null)
                return BadRequest(new { error = "User not found." });

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);

        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string raw_id = HttpContext.User.FindFirstValue("id");
            if(!int.TryParse(raw_id, out int user_id))
            {
                return Unauthorized();
            }

            await _refreshTokenRepository.DeleteAllUserTokens(user_id);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            User user = await _userRepository.GetByName(loginRequest.username);

            if(user == null)
            {
                return Unauthorized();
            }

            bool correctPassword = _passwordHasher.VerifyPassword(loginRequest.password, user.password);

            if (!correctPassword)
                return Unauthorized();

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);
        }
    }
}
