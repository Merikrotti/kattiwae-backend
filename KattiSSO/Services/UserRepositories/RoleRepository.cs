
using KattiSSO.Models;
using KattiSSO.Database;
using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using KattiSSO.Models.Response;

namespace KattiSSO.Services.UserRepositories
{
    public class RoleRepository
    {
        NpgsqlConnection conn;

        /// <summary>
        /// Just opens a connection for now.
        /// </summary>
        public RoleRepository()
        {
            conn = new DbConnection().GetConnection();
        }

        public List<Claim> GetByUser(int user_id)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("SELECT r.name FROM user_roles AS ru, roles AS r WHERE ru.user_id = @user_id AND r.role_id = ru.role_id", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                psqlUser_id.Value = user_id;

                command.Prepare();

                var reader = command.ExecuteReader();

                List<Claim> userRoles = new List<Claim>();
                while (reader.Read())
                {
                    userRoles.Add(new Claim(ClaimTypes.Role, reader.GetString(0)));
                }
                if (userRoles.Count < 1)
                    return null;
                return userRoles;
            }
            catch (Exception e)
            {
                Console.WriteLine("Role getbyuser error: " + e);
            }
            finally
            {
                conn.Close();
            }

            return null;
        }

        public async Task<bool> AddRole(int user_id, string secret)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("INSERT INTO user_roles(user_id, role_id)"
                + " SELECT @user_id, r.role_id FROM roles AS r WHERE secret_token = @secret_token"
                + " AND NOT EXISTS(SELECT * FROM user_roles AS ur WHERE r.role_id = ur.role_id AND ur.user_id = @user_id)", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                var psqlSecret_token = command.Parameters.Add("secret_token", NpgsqlTypes.NpgsqlDbType.Varchar);
                psqlUser_id.Value = user_id;
                psqlSecret_token.Value = secret;

                command.Prepare();

                var reader = await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Role getbyuser error: " + e);
            }
            finally
            {
                conn.Close();
            }

            return false;
        }

        public async Task<List<RoleResponse>> GetAllRoles()
        {
            List<RoleResponse> roles = new List<RoleResponse>();
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("SELECT name, comment FROM roles ORDER BY role_id ASC", conn);

                command.Prepare();

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var newRole = new RoleResponse()
                    {
                        name = reader.GetString(0),
                        comment = reader.GetString(1)
                    };
                    roles.Add(newRole);
                }

                return roles;
            }
            catch (Exception e)
            {
                Console.WriteLine("Role getbyuser error: " + e);
            }
            finally
            {
                conn.Close();
            }

            return roles;
        }
    }
}
