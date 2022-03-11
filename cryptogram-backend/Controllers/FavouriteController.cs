using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavouriteController : Controller
    {
        [Authorize]
        [HttpGet("GetFavourites")]
        public IActionResult GetFavourites()
        {
            return Ok();
        }
    }
}
