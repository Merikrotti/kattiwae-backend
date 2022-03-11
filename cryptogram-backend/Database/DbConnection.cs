using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;

namespace cryptogram_backend.Database
{
    /// <summary>
    /// TODO: Move to services, but for now this shall do.
    /// </summary>
    public class DbConnection
    {
        /// <summary>
        /// Constructor to initiate the SQL connection with YAML.
        /// 
        /// See config.yaml for config style.
        /// </summary>
        public NpgsqlConnection GetConnection()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var myConfig = deserializer.Deserialize<Configuration>(File.ReadAllText("config.yaml"));

            string connString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4}",
                    myConfig.Host,
                    myConfig.User,
                    myConfig.Dbname,
                    myConfig.Port,
                    myConfig.Password);

            var conn = new NpgsqlConnection(connString);

            return conn;
        }
    }

    /// <summary>
    /// Configuration "profile" for the PostgreSQL connection
    /// </summary>
    class Configuration
    {
        public String Host { get; set; }
        public String User { get; set; }
        public String Dbname { get; set; }
        public String Password { get; set; }
        public String Port { get; set; }
    }
}
