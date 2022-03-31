using KattiSSO.Database;
using KattiSSO.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Services.RefreshTokenRepostitory
{
    public class RefreshTokenRepository
    {
        NpgsqlConnection conn;

        /// <summary>
        /// Just opens a connection for now.
        /// </summary>
        public RefreshTokenRepository()
        {
            conn = new DbConnection().GetConnection();
        }

        public async Task<RefreshToken> GetByToken(string token)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("SELECT ref_id, user_id, token FROM refreshtokens WHERE token = @token", conn);

                var psqlToken = command.Parameters.Add("token", NpgsqlTypes.NpgsqlDbType.Varchar);
                psqlToken.Value = token;

                command.Prepare();

                var reader = await command.ExecuteReaderAsync();

                RefreshToken newToken = new RefreshToken();
                while (await reader.ReadAsync())
                {
                    newToken.ref_id = reader.GetInt32(0);
                    newToken.user_id = reader.GetInt32(1);
                    newToken.Token = reader.GetString(2);
                }
                if (newToken.Token == null)
                    newToken = null;
                return newToken;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetByToken error: " + e);
            }
            finally
            {
                conn.Close();
            }

            return null;
        }

        public async Task<bool> DeleteAllUserTokens(int user_id)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("DELETE FROM refreshtokens WHERE user_id = @user_id", conn);

                var psqlRef_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                psqlRef_id.Value = user_id;

                command.Prepare();

                await command.ExecuteNonQueryAsync();

                conn.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Deleting user tokens failed:\n" + e);
            }
            finally
            {
                conn.Close();
            }
            return false;
        }

        public async Task<bool> Delete(int ref_id)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("DELETE FROM refreshtokens WHERE ref_id = @ref_id", conn);

                var psqlRef_id = command.Parameters.Add("ref_id", NpgsqlTypes.NpgsqlDbType.Integer);
                psqlRef_id.Value = ref_id;

                command.Prepare();

                await command.ExecuteNonQueryAsync();

                conn.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Deleting refresh token failed:\n" + e);
            }
            finally
            {
                conn.Close();
            }
            return false;
        }

        public async Task<Task> Create(RefreshToken refreshToken)
        {
            try
            {
                conn.Open();
                var command = new NpgsqlCommand("INSERT INTO refreshtokens(user_id, token) VALUES (@user_id, @token)", conn);
                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                var psqlToken = command.Parameters.Add("token", NpgsqlTypes.NpgsqlDbType.Varchar);

                psqlUser_id.Value = refreshToken.user_id;
                psqlToken.Value = refreshToken.Token;

                command.Prepare();

                await command.ExecuteNonQueryAsync();
                conn.Close();
            } catch (Exception e)
            {
                Console.WriteLine("Could not store refresh token " + e);
            } finally
            {
                conn.Close();
            }
            return Task.CompletedTask;
        }
    }
}
