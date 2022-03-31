using KattiSSO.Database;
using KattiSSO.Models;
using Npgsql;
using System;

using System.Threading.Tasks;

namespace KattiSSO.Services.UserRepositories
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
        public async Task<User> GetById(int user_id)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("SELECT user_id, username, password FROM users WHERE user_id = @user_id", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                psqlUser_id.Value = user_id;

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
            }
            catch (Exception e)
            {
                Console.WriteLine("GetByName error: " + e);
            }
            finally
            {
                conn.Close();
            }

            return null;
        }
        public async Task<User> GetByName(string name)
        {
            try { 
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
