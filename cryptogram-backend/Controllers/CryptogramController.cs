using cryptogram_backend.Database;
using cryptogram_backend.Models;
using cryptogram_backend.Modules;
using cryptogram_backend.Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace cryptogram_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Roles="Secret,Admin")]
    public class CryptogramController : ControllerBase
    {
        FavouriteRepository _favrepo;

        public CryptogramController(FavouriteRepository favrepo)
        {
            _favrepo = favrepo;
        }

        [HttpGet]
        [ActionName("PostUrl")]
        public async Task<IActionResult> PostUrl(string file, String answer)
        {
            if (file == null)
                return BadRequest(new { error = "File parameter is null" });

            if (answer == null)
                return BadRequest(new { error = "Answer parameter is null" });

            if (answer.Length < 4)
                return BadRequest(new { error = "Answer should be more than 4 letters" });

            Scrambler cryptogramScrambler = new Scrambler(answer);

            CryptogramDb database = new CryptogramDb();
            await database.InsertCryptogramData(answer, cryptogramScrambler.ScrambledAnswer, file, "url");

            return Ok(new { posted_answer = answer, scrambled = cryptogramScrambler.ScrambledAnswer});
        }

        [HttpPost]
        [ActionName("PostImage")]
        public async Task<IActionResult> Post(IFormFile file, String answer)
        {
            if (file == null)
                return BadRequest(new { error = "File parameter is null" });

            if (answer == null)
                return BadRequest(new { error = "Answer parameter is null" });

            if (answer.Length < 4)
                return BadRequest(new { error = "Answer should be more than 4 letters"});

            String[] acceptedTypes = { "image/gif", "image/png", "image/jpeg", "video/webm", "video/mp4" };
            var savePath = Path.GetFullPath(AppContext.BaseDirectory + "images/");

            FileSaver imageSaver = new FileSaver(savePath, acceptedTypes);
            await imageSaver.Start(file);

            if (imageSaver.GetStatus())
                return BadRequest(new { error = imageSaver.GetErrorMessage()});

            Scrambler cryptogramScrambler = new Scrambler(answer);

            CryptogramDb database = new CryptogramDb();

            if (imageSaver.GetFileExists())
                return Conflict(new { error = "Image already exists", filename = imageSaver.GetFileName(), contentType = imageSaver.GetMimetype()});

            await database.InsertCryptogramData(answer, cryptogramScrambler.ScrambledAnswer, imageSaver.GetFileName(), imageSaver.GetMimetype());

            return Ok(new { posted_answer = answer, scrambled = cryptogramScrambler.ScrambledAnswer });
        }

        [HttpGet]
        [ActionName("PostExisting")]
        public async Task<IActionResult> PostExisting(String filename, String answer, String contentType)
        {
            if (filename == null)
                return BadRequest(new { error = "File parameter is null" });

            if (answer == null)
                return BadRequest(new { error = "Answer parameter is null" });

            if (answer.Length < 4)
                return BadRequest(new { error = "Answer should be more than 4 letters" });

            String[] acceptedTypes = { "image/gif", "image/png", "image/jpeg", "video/webm", "video/mp4" };

            if (acceptedTypes.Where(c => c == contentType).FirstOrDefault() == null)
                return BadRequest(new { error = "MimeType is wrong" });

            CryptogramDb database = new CryptogramDb();

            Scrambler cryptogramScrambler = new Scrambler(answer);

            await database.InsertCryptogramData(answer, cryptogramScrambler.ScrambledAnswer, filename, contentType);

            return Ok(new { posted_answer = answer, scrambled = cryptogramScrambler.ScrambledAnswer });
        }

        [HttpGet]
        [ActionName("GetHint")]
        public async Task<IActionResult> GetHint(String hint)
        {
            if(hint == null)
                return BadRequest(new { error = "Hint parameter is null" });
            if(hint.Length > 1)
                return BadRequest(new { error = "Hint parameter is not a char" });

            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();

            int index = latest.ScrambledAnswer.IndexOf(hint[0]);
            if(index == -1)
                return BadRequest(new { error = "Char not found, reload scrambled text" });

            return Ok(new { A = latest.ScrambledAnswer[index], B = latest.Answer[index] });
        }

        [HttpGet]
        [ActionName("Compare")]
        public async Task<CryptogramModel> Compare(String answer)
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            if(answer.ToLower() == latest.Answer.ToLower())
                return latest;
            return null;
        }

        [HttpGet]
        [ActionName("HasChanged")]
        public async Task<bool> HasChanged(String answer)
        {
            if (answer == null)
                return false;
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            return (answer.ToLower() == latest.Answer.ToLower());
        }

        [HttpGet]
        [ActionName("GetScramble")]
        public async Task<String> GetScramble()
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            return latest.ScrambledAnswer;
        }


        [HttpGet]
        [ActionName("GetData")]
        public async Task<List<CryptogramModel>> GetData(int page)
        {
            CryptogramDb database = new CryptogramDb();
            List<CryptogramModel> latest = await database.GetData(page);
            if (latest == null)
                return null;
            return latest;
        }

        [HttpGet]
        [ActionName("GetExact")]
        public async Task<CryptogramModel> GetExact(int id)
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel exact = await database.GetExact(id);
            CryptogramModel latest = await database.GetLatest();

            if(exact.Id == latest.Id)
            { 
                string rawid = HttpContext.User.FindFirstValue("id");
                if (!Int32.TryParse(rawid, out int user_id))
                {
                    return new CryptogramModel();
                }

                if (await _favrepo.IsFavourited(user_id, id))
                    return exact;
                else
                    return new CryptogramModel();
            }

            return exact;
        }

        /*
        
        [HttpGet]
        [ActionName("GetLatest")]
        public async Task<CryptogramModel> GetLatest()
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            return latest;
        }
        */
    }

}
