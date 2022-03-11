using cryptogram_backend.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Services.UserRepositories
{
    public interface IUserRepository
    {
        public Task<User> GetByName(string name);

        public Task<User> Create(User user);
    }
}
