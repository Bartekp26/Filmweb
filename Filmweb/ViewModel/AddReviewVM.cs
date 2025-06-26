using Filmweb.ViewModel.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class AddReviewVM : ViewModelBase
    {
        private readonly MainVM _mainVM;

        private string _movieTitle;

        private bool _isEditMode = false;
        private int? _reviewId = null;

        public string MovieTitle
        {
            get => _movieTitle;
            set
            {
                if (_movieTitle != value)
                {
                    _movieTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _rating = 1;
        public double Rating
        {
            get => _rating;
            set
            {
                if (_rating != value)
                {
                    _rating = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CancelCommand => new RelayCommand(_ => Cancel(), _ => true);

        private void Cancel()
        {
            Content = string.Empty;
            Rating = 0;
            _mainVM.NavigateToMovieDetails(MovieTitle);
        }

        public AddReviewVM(MainVM mainVM, string movieTitle)
        {
            _mainVM = mainVM;
            MovieTitle = movieTitle;
            var connection = DatabaseConnection.GetConnection();
            string sql = @"
                    SELECT TOP 1 ID_Opinii, Tresc, Ocena
                    FROM opinie
                    WHERE ID_Filmu = (SELECT ID_Filmu FROM Filmy WHERE Nazwa = @Title)
                      AND ID_Uzytkownika = (SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login)
                    ORDER BY Data_dodania DESC;";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Title", MovieTitle);
                cmd.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _isEditMode = true;
                        _reviewId = Convert.ToInt32(reader["ID_Opinii"]);
                        Content = reader["Tresc"].ToString();
                        Rating = Convert.ToDouble(reader["Ocena"]);
                    }
                }
            }
        }

        public bool SaveReview()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                MessageBox.Show("Treść opinii nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Content.Length > 300)
            {
                MessageBox.Show("Treść opinii przekracza limit 300 znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Rating <= 0 || Rating > 10)
            {
                MessageBox.Show("Musisz wybrać ocenę (od 1 do 10).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            SqlConnection connection = DatabaseConnection.GetConnection();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (_isEditMode && _reviewId.HasValue)
                    {
                        string updateQuery = @"
                                    UPDATE opinie
                                    SET Tresc = @Content, Ocena = @Rating, Data_dodania = GETDATE()
                                    WHERE ID_Opinii = @Id;";

                        using (var cmd = new SqlCommand(updateQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Content", Content);
                            cmd.Parameters.AddWithValue("@Rating", Rating);
                            cmd.Parameters.AddWithValue("@Id", _reviewId.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string insertQuery = @"EXEC dbo.Dodaj_Opinie
                                            @login = @Login,
                                            @nazwa_filmu = @Title,
                                            @tresc = @Content,
                                            @ocena = @Rating;";

                        using (var cmd = new SqlCommand(insertQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Content", Content);
                            cmd.Parameters.AddWithValue("@Rating", Rating);
                            cmd.Parameters.AddWithValue("@Title", MovieTitle);
                            cmd.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    _mainVM.MovieDetailsVM?.Reload();
                    double? userRating = null;

                    using (var command = new SqlCommand(@"
                        SELECT Ocena
                        FROM opinie
                        WHERE ID_Filmu = (SELECT ID_Filmu FROM Filmy WHERE Nazwa = @Title)
                          AND ID_Uzytkownika = (SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login)
                        ORDER BY Data_dodania DESC", connection))
                    {
                        command.Parameters.AddWithValue("@Title", MovieTitle);
                        command.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);

                        var result = command.ExecuteScalar();
                        if (result != null)
                            userRating = Convert.ToDouble(result);
                    }

                    double? updatedAvgRating = null;

                    using (var command = new SqlCommand(@"
                        SELECT AVG(CAST(Ocena AS FLOAT))
                        FROM opinie
                        WHERE ID_Filmu = (SELECT ID_Filmu FROM Filmy WHERE Nazwa = @Title);", connection))
                    {
                        command.Parameters.AddWithValue("@Title", MovieTitle);
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                            updatedAvgRating = Convert.ToDouble(result);
                    }


                    if (userRating.HasValue || updatedAvgRating.HasValue)
                    {
                        _mainVM.HomeVM?.RefreshUserRating(MovieTitle, userRating.Value, updatedAvgRating.Value);
                    }
                        

                    return true;
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
