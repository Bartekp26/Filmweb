using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Filmweb.Model;
using Filmweb.ViewModel.BaseClass;

namespace Filmweb.ViewModel
{
    public class MovieDetailsVM : ViewModelBase
    {
        private readonly MainVM _mainVM;

        private MovieM _movie;
        public MovieM Movie
        {
            get => _movie;
            set
            {
                if (_movie != value)
                {
                    _movie = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(GenresAsText));
                    OnPropertyChanged(nameof(HasUserReview));
                    OnPropertyChanged(nameof(ReviewButtonText));
                }
            }
        }

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
                OnPropertyChanged();
            }
        }

        public bool HasUserReview => Movie?.Reviews.Any(r => r.Author.Username == _mainVM.CurrentUser.Username) == true;
        public string ReviewButtonText => HasUserReview ? "Edytuj opinię" : "Dodaj opinię";

        private bool _isFavourite;
        public bool IsFavourite
        {
            get => _isFavourite;
            set
            {
                if (_isFavourite != value)
                {
                    _isFavourite = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HeartIcon));
                }
            }
        }

        public string HeartIcon => IsFavourite ? "\uEB52" : "\uEB51";

        public ICommand AddReviewCommand => _mainVM.AddReviewCommand;
        public ICommand ToggleFavouriteCommand { get; }

        public MovieDetailsVM(string title, MainVM mainVM)
        {
            _mainVM = mainVM;
            ToggleFavouriteCommand = new RelayCommand(_ => ToggleFavourite(), p => true);
            LoadMovieFromDatabase(title);
            LoadMovieReviews(title);
            LoadIsFavourite();
        }

        public void Reload()
        {
            var t = Movie?.Title;
            Movie = null;

            LoadMovieFromDatabase(t);
            LoadMovieReviews(t);
            LoadIsFavourite();
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
                            (
                                SELECT AVG(CAST(Ocena AS FLOAT))
                                FROM opinie O
                                WHERE O.ID_Filmu = F.ID_Filmu
                            ) AS Rating,
                            (
                                SELECT DISTINCT G.Gatunek + ', '
                                FROM Conn_Filmy_Gat C2
                                JOIN Gatunek G ON G.ID_Gatunku = C2.ID_Gatunku
                                WHERE C2.ID_Filmu = F.ID_Filmu
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)') AS Genres,
                            (
                                SELECT DISTINCT A.Imie + ' ' + A.Nazwisko + ', '
                                FROM Con_Filmy_Osoby C
                                JOIN Aktor A ON A.ID_Aktora = C.ID_Aktora
                                WHERE C.ID_Filmu = F.ID_Filmu AND A.ID_Aktora IS NOT NULL
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)') AS Actors,
                            (
                                SELECT DISTINCT R.Imie + ' ' + R.Nazwisko + ', '
                                FROM Con_Filmy_Osoby C
                                JOIN Rezyser R ON R.ID_Rezysera = C.ID_Rezysera
                                WHERE C.ID_Filmu = F.ID_Filmu AND R.ID_Rezysera IS NOT NULL
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)') AS Directors
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
                            // ImageUrl = reader["ImageUrl"].ToString(),
                            Genres = reader["Genres"]?.ToString()
                                     ?.Split(',')
                                     .Select(g => g.Trim())
                                     .Where(g => !string.IsNullOrWhiteSpace(g))
                                     .ToList(),
                            Actors = reader["Actors"]?.ToString()
                                     ?.Split(',')
                                     .Select(a => a.Trim())
                                     .Where(a => !string.IsNullOrWhiteSpace(a))
                                     .ToList(),
                            Directors = reader["Directors"]?.ToString()
                                     ?.Split(',')
                                     .Select(d => d.Trim())
                                     .Where(d => !string.IsNullOrWhiteSpace(d))
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
                    Movie.Reviews = new List<ReviewM>();
                    while (reader.Read())
                    {
                        if (reader["Content"] != DBNull.Value)
                        {
                            var review = new ReviewM
                            {
                                Content = reader["Content"].ToString(),
                                Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : 0,
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

            OnPropertyChanged(nameof(HasUserReview));
            OnPropertyChanged(nameof(ReviewButtonText));
        }

        private void ToggleFavourite()
        {
            SqlConnection connection = DatabaseConnection.GetConnection();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string query;

                    if (IsFavourite)
                    {
                        query = @"
                     DELETE FROM Fav_Filmy
                     WHERE ID_Uzytkownika = (SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login)
                     AND ID_Filmu = (SELECT ID_Filmu FROM Filmy WHERE Nazwa = @Title)";
                    }
                    else
                    {
                        query = @"
                     INSERT INTO Fav_Filmy (ID_Filmu, ID_Uzytkownika)
                     VALUES (
                         (SELECT ID_Filmu FROM Filmy WHERE Nazwa = @Title),
                         (SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login)
                     )";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);
                        command.Parameters.AddWithValue("@Title", Movie.Title);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            LoadIsFavourite();
        }
        private void LoadIsFavourite()
        {
            var connection = DatabaseConnection.GetConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string query = @"
                        SELECT ISNULL((
                            SELECT 1
                            FROM Fav_Filmy FF
                            INNER JOIN Filmy F ON F.ID_Filmu = FF.ID_Filmu
                            WHERE FF.ID_Uzytkownika = (
                                SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login
                            )
                            AND F.Nazwa = @Title
                        ), 0);";

                    using (var cmd = new SqlCommand(query, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);
                        cmd.Parameters.AddWithValue("@Title", Movie.Title);
                        var result = cmd.ExecuteScalar();
                        IsFavourite = (result != null && Convert.ToInt32(result) == 1);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
