using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Services.PasswordHasher
{
    public class BcryptHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string hashedpassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedpassword);
        }
    }
}
