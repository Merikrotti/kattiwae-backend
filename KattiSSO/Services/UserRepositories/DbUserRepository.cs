using cryptogram_backend.Database;
using cryptogram_backend.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Services.UserRepositories
{
    public class DbUserRepository : IUserRepository
    {
        NpgsqlConnection conn;

        /// <summary>
        /// Just opens a connection for now.
        /// </summary>
        public DbUserRepository()
        {
            conn = new DbConnection().GetConnection();
        }

        public async Task<User> GetByName(string name)
        {
            try { 
                var conn = new DbConnection().GetConnection();
                conn.Open();
                var command = new NpgsqlCommand("SELECT user_id, username, password FROM users WHERE LOWER(username) = @username", conn);

                var psqlUsername = command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar);
                psqlUsername.Value = name.ToLower();

                command.Prepare();

                var reader = await command.ExecuteReaderAsync();

                User newUser = new User();
                while (await reader.ReadAsync())
                {
                    newUser.user_id = reader.GetInt32(0);
                    newUser.username = reader.GetString(1);
                    newUser.password = reader.GetString(2);
                }
                if (newUser.username == null)
                    newUser = null;
                return newUser;
            } catch (Exception e)
            {
                Console.WriteLine("GetByName error: " + e);
            } finally
            {
                conn.Close();
            }

            return null;
        }

        public async Task<User> Create(User user)
        {
            conn.Open();
            var command = new NpgsqlCommand("INSERT INTO users(username, password) VALUES (@username, @password)", conn);
            var psqlUsername = command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar);
            var psqlPassword = command.Parameters.Add("password", NpgsqlTypes.NpgsqlDbType.Varchar);

            psqlUsername.Value = user.username;
            psqlPassword.Value = user.password;

            command.Prepare();

            await command.ExecuteNonQueryAsync();
            conn.Close();

            return user;
        }
    }
}
