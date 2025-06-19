using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.ViewModel
{

    public class MovieDetailsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MovieM Movie { get; private set; }

        public string Title => Movie?.Title;
        public string Description => Movie?.Description;
        public string GenresAsText => string.Join(", ", Movie?.Genres ?? new List<string>());

        private bool _isImageVisible = true;
        public bool IsImageVisible
        {
            get => _isImageVisible;
            set
            {
                _isImageVisible = value;
                OnPropertyChanged(nameof(IsImageVisible));
            }
        }

        public MovieDetailsVM(string title)
        {
            LoadMovieFromDatabase(title);
            LoadMovieReviews(title);
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
                                .ToList(),
                            Reviews = new List<ReviewM>()
                        };
                    }
                }
            }
        }

        private void LoadMovieReviews(string title)
        {
            var connection = DatabaseConnection.GetConnection();

            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            string sql = @"
                        SELECT 
                            R.Tresc as Content,
                            R.Ocena as Rating,
                            R.Data_dodania as DateAdded,
                            (
                                SELECT L.Login
                                FROM UZ_Login L
                                WHERE L.ID_Uzytkownika=R.ID_Uzytkownika
                            ) as Author
                        FROM Filmy F
                        LEFT JOIN opinie R ON F.ID_Filmu=R.ID_Filmu
                        WHERE F.Nazwa = @title;";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@title", title);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Content"] != DBNull.Value)
                        {
                            var review = new ReviewM
                            {
                                Content = reader["Content"].ToString(),
                                Rating = reader["Rating"] != DBNull.Value ? Convert.ToDouble(reader["Rating"]) : 0,
                                DateAdded = reader["DateAdded"] != DBNull.Value ? Convert.ToDateTime(reader["DateAdded"]) : DateTime.MinValue,
                                Author = new UserM
                                {
                                    Username = reader["Author"]?.ToString() ?? "Anonim"
                                }
                            };

                            Movie.Reviews.Add(review);
                        }
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

