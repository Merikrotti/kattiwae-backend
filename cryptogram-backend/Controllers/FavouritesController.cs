using cryptogram_backend.Database;
using cryptogram_backend.Models;
using cryptogram_backend.Models.Requests;
using cryptogram_backend.Models.Responses;
using cryptogram_backend.Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace cryptogram_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Secret,Admin")]
    public class FavouritesController : Controller
    {
        FavouriteRepository _favouriteRepository;

        public FavouritesController(FavouriteRepository favouriteRepository)
        {
            _favouriteRepository = favouriteRepository;
        }

        [HttpGet("GetAccountData")]
        public IActionResult GetAccountData()
        {
            string id = HttpContext.User.FindFirstValue("id");
            string name = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            return Ok(new { id = id, name = name });
        }

        [HttpGet("getfavourites")]
        public async Task<IActionResult> GetFavourites(int page = -1)
        {
            if (page < -1)
                return BadRequest();
            string rawid = HttpContext.User.FindFirstValue("id");

            if (!Int32.TryParse(rawid, out int user_id))
            {
                return Unauthorized();
            }

            var favourites = await _favouriteRepository.GetFavourites(user_id, page);

            if (favourites == null)
                return NoContent();

            var total = await _favouriteRepository.getTotalFavourites(user_id);
   
            return Ok(new { favourites=favourites, total=total });
        }

        [HttpPost("isfavourited")]
        public async Task<IActionResult> IsFavourited(PostFavourite request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            var rawid = HttpContext.User.FindFirstValue("id");

            if (!Int32.TryParse(rawid, out int user_id))
            {
                return Unauthorized();
            }

            var result = await _favouriteRepository.IsFavourited(user_id, request.img_id);

            return Ok(new { result = result });
        }
        
        [HttpPost("removefavourite")]
        public async Task<IActionResult> RemoveFavourite([FromBody] PostFavourite request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var rawid = HttpContext.User.FindFirstValue("id");

            if (!Int32.TryParse(rawid, out int user_id))
            {
                return Unauthorized();
            }

            var success = await _favouriteRepository.RemoveFavourite(request.img_id, user_id);
            if (!success)
                return BadRequest();

            return Ok();
        }

        [HttpPost("postfavourite")]
        public async Task<IActionResult> PostFavourite([FromBody] PostFavourite request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string rawid = HttpContext.User.FindFirstValue("id");

            if(!Int32.TryParse(rawid, out int user_id)) {
                return Unauthorized();
            }

            var success = await _favouriteRepository.PostFavourite(request.img_id, user_id);

            if (!success)
                return BadRequest(new {error = "Error adding favourite"});

            return Ok();
        }

        [HttpGet("GetDataWithFav")]
        public async Task<IActionResult> GetDataWithFav(int page)
        {
            if (page < 0)
                return BadRequest();

            var response = await _favouriteRepository.GetData(page);

            if (response == null)
                return BadRequest(new {error = "Error fetching favourite data"});

            return Ok(response);
        }
    }
}
