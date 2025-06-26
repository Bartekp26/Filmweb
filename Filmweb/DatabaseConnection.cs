using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System;
using System.Windows;

namespace Filmweb
{
    internal static class DatabaseConnection
    {
        private static SqlConnection _connection;

        public static SqlConnection GetConnection()
        {
            try
            {
                if (_connection == null)
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string configPath = Path.Combine(baseDir, "..", "..", "dbconfig.txt");
                    var config = LoadConfig(configPath);

                    string ip = config["ip"];
                    string port = config["port"];
                    string database = config["database"];
                    string user = config["user"];
                    string password = config["password"];

                    string connectionString = $"Server={ip},{port};Database={database};User Id={user};Password={password};";
                    _connection = new SqlConnection(connectionString);
                }

                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }

                return _connection;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się połączyć z bazą danych:\n{ex.Message}", "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; 
            }
        }
        private static Dictionary<string, string> LoadConfig(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Nie znaleziono pliku konfiguracyjnego: {path}");

            var config = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(path))
            {
                if (line.Trim().Length == 0 || line.StartsWith("#")) continue;

                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return config;
        }
    }
}
