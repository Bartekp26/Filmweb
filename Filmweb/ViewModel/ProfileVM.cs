using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Filmweb.ViewModel
{
    public class ProfileVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;

        public UserM CurrentUser => _mainVM.CurrentUser;

        public ICommand NavigateToEditProfileCommand => _mainVM.NavigateToEditProfileCommand;

        public ProfileVM(MainVM mainVM)
        {
            _mainVM = mainVM;
            _mainVM.PropertyChanged += MainVM_PropertyChanged;
            LoadUserReviews();
            LoadFavouriteMovies();
        }

        public void LoadFavouriteMovies()
        {
            SqlConnection connection = DatabaseConnection.GetConnection();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string favmoviequery = @"
                            SELECT STRING_AGG(F.Nazwa, ', ')
                            FROM Fav_Filmy FF
                            INNER JOIN Filmy F ON F.ID_Filmu = FF.ID_Filmu
                            WHERE FF.ID_Uzytkownika = (
                                SELECT ID_Uzytkownika
                                FROM UZ_Login
                                WHERE Login = @Login
                            );";

                    using (SqlCommand command = new SqlCommand(favmoviequery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);

                        var result = command.ExecuteScalar();
                        _mainVM.CurrentUser.FavouriteMovies = result != DBNull.Value ? result.ToString() : string.Empty;
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

        public ObservableCollection<ReviewM> UserReviews { get; } = new ObservableCollection<ReviewM>();

        public void LoadUserReviews()
        {
            UserReviews.Clear();
            var connection = DatabaseConnection.GetConnection();

            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            string sql = @"
                        SELECT 
                            F.Nazwa AS FilmName,
                            R.Tresc AS Content,
                            R.Ocena AS Rating,
                            R.Data_dodania AS DateAdded
                        FROM opinie R
                        JOIN Filmy F ON R.ID_Filmu = F.ID_Filmu
                        WHERE R.ID_Uzytkownika = (
                            SELECT ID_Uzytkownika FROM UZ_Login WHERE Login = @Login
                        )
                        ORDER BY R.Data_dodania DESC;";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserReviews.Add(new ReviewM
                        {
                            FilmName = reader["FilmName"]?.ToString(),
                            Content = reader["Content"]?.ToString(),
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : 0,
                            DateAdded = reader["DateAdded"] != DBNull.Value ? Convert.ToDateTime(reader["DateAdded"]) : DateTime.MinValue,
                            Author = _mainVM.CurrentUser
                        });
                    }
                }
            }
        }

        private void MainVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainVM.CurrentUser))
            {
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
