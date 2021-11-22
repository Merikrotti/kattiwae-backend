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

        public CryptogramDb()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            System.Diagnostics.Debug.WriteLine(File.ReadAllText("config.yaml"));
            var myConfig = deserializer.Deserialize<Configuration>(File.ReadAllText("config.yaml"));

            string connString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4}",
                    myConfig.Host,
                    myConfig.User,
                    myConfig.Dbname,
                    myConfig.Port,
                    myConfig.Password);

            conn = new NpgsqlConnection(connString);
        }

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

        public async Task<List<CryptogramModel>> GetLast50()
        {
            conn.Open();
            var command = new NpgsqlCommand("SELECT id, answer, scrambled, filename, contenttype FROM cryptogram ORDER BY id DESC LIMIT 50", conn);
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
    }
    class Configuration
    {
        public String Host { get; set; }
        public String User { get; set; }
        public String Dbname { get; set; }
        public String Password { get; set; }
        public String Port { get; set; }
    }
}
