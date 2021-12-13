using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace cryptogram_backend.Modules
{
    /// <summary>
    /// A file save function for image saving and metadata processing.
    /// </summary>
    public class FileSaver
    {
        private String savepath;
        private String[] acceptedTypes;
        private String errorMessage;
        private bool failed;
        private bool fileExists;
        private String newFileName = "";
        private String mimeType = "";

        /// <summary>
        /// Constructor for save path and the accepted mimetypes
        /// </summary>
        /// <param name="_savepath">Where to save the files</param>
        /// <param name="_acceptedTypes">What mimetypes are allowed</param>
        public FileSaver(String _savepath, String[] _acceptedTypes)
        {
            acceptedTypes = _acceptedTypes;
            savepath = _savepath;
            failed = false;
            errorMessage = "";
            fileExists = false;
        }

        /// <summary>
        /// Starts the saving and data gathering of the file.
        /// </summary>
        /// <param name="file">File to save</param>
        /// <returns>Nothing</returns>
        public async Task Start(IFormFile file)
            => await SaveFile(file);

        /// <summary>
        /// Returns the mimetype of the file
        /// </summary>
        /// <returns>String</returns>
        public String GetMimetype()
            => mimeType;

        /// <summary>
        /// Returns if the saving was a success
        /// </summary>
        /// <returns>Boolean</returns>
        public bool GetStatus()
            => failed;

        /// <summary>
        /// Returns the new filename
        /// </summary>
        /// <returns>String</returns>
        public String GetFileName()
            => newFileName;

        /// <summary>
        /// Returns if the file already existed
        /// </summary>
        /// <returns></returns>
        public bool GetFileExists()
            => fileExists;

        /// <summary>
        /// Returns the error message if any
        /// </summary>
        /// <returns>String</returns>
        public String GetErrorMessage()
            => errorMessage;

        /// <summary>
        /// Checks if the file is too big, an accepted mimetype and saves it. (.tmp -> proper file extension)
        /// 
        /// Uses MD5 to hash the image to avoid duplicates. (Though might be a bit useless)
        /// </summary>
        /// <param name="file">File to save</param>
        /// <returns>Nothing</returns>
        private async Task SaveFile(IFormFile file)
        {
            long size = file.Length;

            if (size < 50 || acceptedTypes.Where(c => c == file.ContentType).FirstOrDefault() == null)
                errorMessage = "Not an image";

            if (size > 104857600)
                errorMessage = "Size too large (>100MB)";

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

        /// <summary>
        /// Lazy way to translate the mimetypes for the React app. Makes my life easier.
        /// Example: jpeg is .jpg etc
        /// </summary>
        /// <param name="contentType">Content type of the file</param>
        /// <returns>String</returns>
        private String MimeTypeTranslator(String contentType)
        {
            switch(contentType)
            {
                case "image/png":
                    mimeType = "image";
                    return ".png";
                case "image/jpeg":
                    mimeType = "image";
                    return ".jpg";
                case "image/gif":
                    mimeType = "image";
                    return ".gif";
                case "video/mp4":
                    mimeType = "video/mp4";
                    return ".mp4";
                case "video/webm":
                    mimeType = "video/webm";
                    return ".webm";
                default:
                    failed = true;
                    errorMessage = "MimeType not defined";
                    return ".fail";
            }
        }

        /// <summary>
        /// Hashes a file by its name
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns>MD5 hash (String) of the file</returns>
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
