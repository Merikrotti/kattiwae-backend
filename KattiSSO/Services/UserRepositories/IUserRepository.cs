using KattiSSO.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Services.UserRepositories
{
    public interface IUserRepository
    {
        public Task<User> GetById(int user_id);
        public Task<User> GetByName(string name);

        public Task<User> Create(User user);
    }
}
