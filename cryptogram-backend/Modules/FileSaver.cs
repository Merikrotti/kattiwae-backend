using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace cryptogram_backend.Modules
{
    public class FileSaver
    {
        private String savepath;
        private String[] acceptedTypes;
        private String errorMessage;
        private bool failed;
        private bool fileExists;
        private String newFileName = "";

        public FileSaver(String _savepath, String[] _acceptedTypes)
        {
            acceptedTypes = _acceptedTypes;
            savepath = _savepath;
            failed = false;
            errorMessage = "";
            fileExists = false;
        }

        public async Task Start(IFormFile file)
            => await SaveFile(file);

        public bool GetStatus()
            => failed;

        public String GetFileName()
            => newFileName;

        public bool GetFileExists()
            => fileExists;

        public String GetErrorMessage()
            => errorMessage;

        private async Task SaveFile(IFormFile file)
        {
            long size = file.Length;

            if (size < 50 || acceptedTypes.Where(c => c == file.ContentType).FirstOrDefault() == null)
                errorMessage = "Not an image";

            if (size > 31457280)
                errorMessage = "Size too large (>30MB)";

            if(errorMessage != "")
            {
                this.failed = true;
                return;
            }

            var filePath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            //Combine save path, mimetype and new filename (md5)

            var fileExtension = MimeTypeTranslator(file.ContentType);

            

            newFileName = GetMD5String(filePath) + fileExtension;

            if (failed)
                return;

            if (System.IO.File.Exists(savepath + newFileName))
                fileExists = true;
            else
                System.IO.File.Move(filePath, savepath + newFileName);
        }

        private String MimeTypeTranslator(String contentType)
        {
            switch(contentType)
            {
                case "image/png":
                    return ".png";
                case "image/jpeg":
                    return ".jpg";
                case "image/gif":
                    return ".gif";
                default:
                    failed = true;
                    errorMessage = "MimeType not defined";
                    return ".fail";
            }
        }

        private String GetMD5String(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
