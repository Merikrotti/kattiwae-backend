using cryptogram_backend.Database;
using cryptogram_backend.Models.Responses;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cryptogram_backend.Services.Repository
{
    public class FavouriteRepository
    {

        public async Task<bool> IsFavourited(int user_id, int img_id)
        {
            var conn = new DbConnection().GetConnection();
            try
            {
                await conn.OpenAsync();
                NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM favourites WHERE user_id = @user_id AND img_id = @img_id", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                var psqlImg_id = command.Parameters.Add("img_id", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlImg_id.Value = img_id;
                psqlUser_id.Value = user_id;

                var reader = await command.ExecuteReaderAsync();

                int total = 0;
                while (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }

                if (total < 1)
                    return false;

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in getTotalFavourites, " + e.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return false;
        }

        public async Task<List<PreviousPageAuthorizedResponse>> GetData(int page) {

            List<PreviousPageAuthorizedResponse> list = new List<PreviousPageAuthorizedResponse>();
            var conn = new DbConnection().GetConnection();
            try
            {
                
                await conn.OpenAsync();

                var command = new NpgsqlCommand("SELECT c.id, c.answer, c.filename, COUNT(f.img_id) AS fav_count"
                                                + " FROM cryptogram AS c"
                                                + " LEFT JOIN favourites AS f"
                                                + " ON c.id = f.img_id"
                                                + " GROUP BY c.id"
                                                + " ORDER BY c.id DESC"
                                                + " LIMIT 50 OFFSET 1 + 50 * @page", conn);

                var psqlPage = command.Parameters.Add("page", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlPage.Value = page;

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var newItem = new PreviousPageAuthorizedResponse()
                    {
                        img_id = reader.GetInt32(0),
                        answer = reader.GetString(1),
                        filename = reader.GetString(2),
                        fav_count = reader.GetInt32(3)
                    };
                    list.Add(newItem);
                }
                if (list.Count < 1)
                    return null;

                return list;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PostFavourite, " + e.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }

        public async Task<List<GetFavouriteResponse>> GetFavourites(int user_id, int page)
        {
            List<GetFavouriteResponse> list = new List<GetFavouriteResponse>();
            var conn = new DbConnection().GetConnection();
            try
            {
                await conn.OpenAsync();
                NpgsqlCommand command;

                if (page == -1)
                    command = new NpgsqlCommand("SELECT c.id, c.answer, c.filename FROM cryptogram AS c, favourites AS f WHERE c.id = f.img_id AND f.user_id = @user_id ORDER BY c.id DESC", conn);
                else {
                    command = new NpgsqlCommand("SELECT c.id, c.answer, c.filename, COUNT(c.id) OVER() AS total FROM cryptogram AS c, favourites AS f WHERE c.id = f.img_id AND f.user_id = @user_id ORDER BY c.id DESC LIMIT 50 OFFSET 50*@page", conn);
                    var psqlPage = command.Parameters.Add("page", NpgsqlTypes.NpgsqlDbType.Integer);

                    psqlPage.Value = page;
                }

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlUser_id.Value = user_id;

                var reader = await command.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {
                    var newFav = new GetFavouriteResponse()
                    {
                        img_id = reader.GetInt32(0),
                        answer = reader.GetString(1),
                        filename = reader.GetString(2)
                    };
                    list.Add(newFav);
                }
                if (list.Count < 1)
                    return null;

                return list;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PostFavourite, " + e.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return null;
        }

        public async Task<int> getTotalFavourites(int user_id)
        {
            var conn = new DbConnection().GetConnection();
            try
            {
                await conn.OpenAsync();
                NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM favourites WHERE user_id = @user_id", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlUser_id.Value = user_id;

                var reader = await command.ExecuteReaderAsync();

                int total = 0;
                while (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);   
                }

                return total;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in getTotalFavourites, " + e.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return 0;
        }

        public async Task<bool> PostFavourite(int img_id, int user_id)
        {
            var conn = new DbConnection().GetConnection();
            try
            {
                await conn.OpenAsync();

                var command = new NpgsqlCommand("INSERT INTO favourites(img_id, user_id)"
                                               +" SELECT c.id, @user_id FROM cryptogram AS c WHERE c.id = @img_id AND NOT EXISTS"
                                               +" (SELECT * FROM favourites AS f WHERE f.user_id = @user_id AND f.img_id = @img_id)", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                var psqlImg_id = command.Parameters.Add("img_id", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlImg_id.Value = img_id;
                psqlUser_id.Value = user_id;

                await command.ExecuteNonQueryAsync();

                return true;

            } catch (Exception e)
            {
                Console.WriteLine("Error in PostFavourite, " + e.Message);
            } finally
            {
                await conn.CloseAsync();
            }
            return false;
        }
        public async Task<bool> RemoveFavourite(int img_id, int user_id)
        {
            var conn = new DbConnection().GetConnection();
            try
            {
                await conn.OpenAsync();

                var command = new NpgsqlCommand(@"DELETE FROM favourites 
                                                  WHERE user_id = @user_id AND img_id = @img_id", conn);

                var psqlUser_id = command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Integer);
                var psqlImg_id = command.Parameters.Add("img_id", NpgsqlTypes.NpgsqlDbType.Integer);

                psqlImg_id.Value = img_id;
                psqlUser_id.Value = user_id;

                await command.ExecuteNonQueryAsync();

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in RemoveFavourite, " + e.Message);
            }
            finally
            {
                await conn.CloseAsync();
            }
            return false;
        }
    }
}
