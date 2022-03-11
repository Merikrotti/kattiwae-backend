using cryptogram_backend.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;

namespace cryptogram_backend.Database
{
    public class CryptogramDb
    {
        

        NpgsqlConnection conn;

        /// <summary>
        /// Just opens a connection for now.
        /// </summary>
        public CryptogramDb()
        {
            conn = new DbConnection().GetConnection();
        }

        /// <summary>
        /// Inserts data into the cryptogram database
        /// 
        /// The data should be processed first by FileSaver and Scrambler!
        /// </summary>
        /// <param name="answer">Cryptogram answer</param>
        /// <param name="scrambled">Scrambled answer</param>
        /// <param name="filename">File name</param>
        /// <param name="contentType">Content type</param>
        /// <returns></returns>
        public async Task InsertCryptogramData(String answer, String scrambled, String filename, String contentType)
        {
            conn.Open();
            var command = new NpgsqlCommand("INSERT INTO cryptogram(answer, scrambled, filename, contenttype) VALUES (@answer, @scrambled, @filename, @contenttype)", conn);
            var psqlAnswer = command.Parameters.Add("answer", NpgsqlTypes.NpgsqlDbType.Varchar);
            var psqlScrambled = command.Parameters.Add("scrambled", NpgsqlTypes.NpgsqlDbType.Varchar);
            var psqlFilename = command.Parameters.Add("filename", NpgsqlTypes.NpgsqlDbType.Varchar);
            var psqlContenttype = command.Parameters.Add("contenttype", NpgsqlTypes.NpgsqlDbType.Varchar);

            psqlAnswer.Value = answer;
            psqlScrambled.Value = scrambled;
            psqlFilename.Value = filename;
            psqlContenttype.Value = contentType;

            command.Prepare();

            await command.ExecuteNonQueryAsync();
            conn.Close();
        }

        /// <summary>
        /// Gets the latest item in the cryptogram database
        /// </summary>
        /// <returns>CryptogramModel</returns>
        public async Task<CryptogramModel> GetLatest()
        {
            conn.Open();
            var command = new NpgsqlCommand("SELECT id, answer, scrambled, filename, contenttype FROM cryptogram ORDER BY id DESC LIMIT 1", conn);
            var reader = await command.ExecuteReaderAsync();

            CryptogramModel newModel = new CryptogramModel();
            
            while (await reader.ReadAsync())
            {
                newModel.Id = reader.GetInt32(0);
                newModel.Answer = reader.GetString(1);
                newModel.ScrambledAnswer = reader.GetString(2);
                newModel.ImageName = reader.GetString(3);
                newModel.ContentType = reader.GetString(4);
            }

            conn.Close();
            return newModel;
        }

        /// <summary>
        /// Gets 50 last cryptograms saved into the database by page. Newest one hidden
        /// </summary>
        /// <param name="page">Which block of 50 are we looking at. (50 * page)</param>
        /// <returns>List of 50 items of CryptogramModel</returns>
        public async Task<List<CryptogramModel>> GetData(int page)
        {
            int latest = GetLatest().Result.Id;
            int index = latest - page * 50;
            if (index < 1)
                return null;

            conn.Open();
            var command = new NpgsqlCommand("SELECT id, answer, scrambled, filename, contenttype FROM cryptogram WHERE id < @index ORDER BY id DESC LIMIT 50", conn);
            var psqlIndex = command.Parameters.Add("index", NpgsqlTypes.NpgsqlDbType.Integer);

            psqlIndex.Value = index;
            command.Prepare();

            var reader = await command.ExecuteReaderAsync();
            
            List<CryptogramModel> modelList = new List<CryptogramModel>();
            while (await reader.ReadAsync())
            {
                CryptogramModel newModel = new CryptogramModel();
                newModel.Id = reader.GetInt32(0);
                newModel.Answer = reader.GetString(1); 
                newModel.ScrambledAnswer = reader.GetString(2);
                newModel.ImageName = reader.GetString(3);
                newModel.ContentType = reader.GetString(4);
                modelList.Add(newModel);
            }

            conn.Close();
            return modelList;
        }

        /// <summary>
        /// Gets the exact item by id from cryptogram database
        /// </summary>
        /// <param name="id">Database index</param>
        /// <returns>CryptogramModel</returns>
        public async Task<CryptogramModel> GetExact(int id)
        {
            int latest = GetLatest().Result.Id;
            if (id >= latest || id < 1)
                return null;

            conn.Open();
            var command = new NpgsqlCommand("SELECT id, answer, scrambled, filename, contenttype FROM cryptogram WHERE id = @index", conn);
            var psqlIndex = command.Parameters.Add("index", NpgsqlTypes.NpgsqlDbType.Integer);

            psqlIndex.Value = id;
            command.Prepare();

            var reader = await command.ExecuteReaderAsync();

            CryptogramModel newModel = new CryptogramModel();
            while (await reader.ReadAsync())
            {
                newModel.Id = reader.GetInt32(0);
                newModel.Answer = reader.GetString(1);
                newModel.ScrambledAnswer = reader.GetString(2);
                newModel.ImageName = reader.GetString(3);
                newModel.ContentType = reader.GetString(4);
            }

            conn.Close();
            return newModel;
        }
    }

}
