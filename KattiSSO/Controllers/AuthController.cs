using cryptogram_backend.Models;
using cryptogram_backend.Models.Request;
using cryptogram_backend.Models.Response;
using cryptogram_backend.Services.PasswordHasher;
using cryptogram_backend.Services.TokenGenarator;
using cryptogram_backend.Services.UserRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace cryptogram_backend.Controllers
{
    /// <summary>
    /// JWT Authentication controller
    /// 
    /// Created with help of
    /// https://www.youtube.com/watch?v=vNaDR2fPLKU
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AccessTokenGenerator _tokenGenerator;

        public AuthController(IUserRepository userRepository, IPasswordHasher pwhash, AccessTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = pwhash;
            _tokenGenerator = tokenGenerator;
        }

        [Authorize]
        [HttpGet("GetAccountData")]
        public IActionResult GetAccountData()
        {
            string id = HttpContext.User.FindFirstValue("id");
            string name = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            return Ok(new { id = id, name = name});
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

            string accessToken = _tokenGenerator.GenerateToken(user);

            return Ok(new AuthenticatedUserResponse()
            {
                AccessToken = accessToken
            });
        }
    }
}
