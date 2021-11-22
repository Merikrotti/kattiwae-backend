using cryptogram_backend.Database;
using cryptogram_backend.Models;
using cryptogram_backend.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace cryptogram_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CryptogramController : ControllerBase
    {
        [HttpPost]
        [ActionName("PostImage")]
        public async Task<IActionResult> Post(IFormFile file, String answer)
        {
            if (answer.Length < 4)
                return BadRequest(new { error = "Answer should be more than 4 letters"});

            String[] acceptedTypes = { "image/gif", "image/png", "image/jpeg" };
            var savePath = Path.GetFullPath(AppContext.BaseDirectory + "images/");

            FileSaver imageSaver = new FileSaver(savePath, acceptedTypes);
            await imageSaver.Start(file);

            if (imageSaver.GetStatus())
                return BadRequest(new { error = imageSaver.GetErrorMessage()});

            Scrambler cryptogramScrambler = new Scrambler(answer);

            CryptogramDb database = new CryptogramDb();
            await database.InsertCryptogramData(answer, cryptogramScrambler.ScrambledAnswer, imageSaver.GetFileName());

            return Ok(new { posted_answer = answer, scrambled = cryptogramScrambler.ScrambledAnswer, file_exists = imageSaver.GetFileExists() });
        }

        [HttpGet]
        [ActionName("GetHint")]
        public async Task<IActionResult> GetHint()
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();

            int index = new Random().Next(0, latest.Answer.Length - 1);

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
        [ActionName("GetScramble")]
        public async Task<String> GetScramble()
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            return latest.ScrambledAnswer;
        }

        [HttpGet]
        [ActionName("GetLast50")]
        public async Task<List<CryptogramModel>> GetLast50()
        {
            CryptogramDb database = new CryptogramDb();
            List<CryptogramModel> latest = await database.GetLast50();
            return latest;
        }

        [HttpGet]
        [ActionName("GetLatest")]
        public async Task<CryptogramModel> GetLatest()
        {
            CryptogramDb database = new CryptogramDb();
            CryptogramModel latest = await database.GetLatest();
            return latest;
        }
    }

}
