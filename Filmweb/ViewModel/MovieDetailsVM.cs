using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.ViewModel
{

    public class MovieDetailsVM
    {
        public MovieM Movie { get; private set; }

        public string Title => Movie?.Title;
        public string Description => Movie?.Description;
        public string GenresAsText => string.Join(", ", Movie?.Genres ?? new List<string>());

        public MovieDetailsVM(string title)
        {
            LoadMovieFromDatabase(title);
        }

        private void LoadMovieFromDatabase(string title)
        {
            var connection = DatabaseConnection.GetConnection();

            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            string sql = @"
                        SELECT 
                            F.Nazwa AS Title,
                            F.Opis AS Description,
                            F.Ocena AS Rating,
                            F.url AS ImageUrl,
                            (
                                SELECT DISTINCT G.Gatunek + ', '
                                FROM Conn_Filmy_Gat C2
                                JOIN Gatunek G ON G.ID_Gatunku = C2.ID_Gatunku
                                WHERE C2.ID_Filmu = F.ID_Filmu
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)') AS Genres
                        FROM Filmy F
                        WHERE F.Nazwa = @title;";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@title", title);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Movie = new MovieM
                        {
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Rating = double.TryParse(reader["Rating"]?.ToString(), out var rating) ? rating : 0,
                            ImageUrl = reader["ImageUrl"].ToString(),
                            Genres = reader["Genres"]?.ToString()
                                ?.Split(',')
                                .Select(g => g.Trim())
                                .Where(g => !string.IsNullOrWhiteSpace(g))
                                .ToList()
                        };
                    }
                }
            }
        }
    }
}

