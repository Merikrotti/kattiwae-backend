using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Services.PasswordHasher
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string hashedpassword);
    }
}
