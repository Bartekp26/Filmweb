using Filmweb.Model;
using System;
using System.Collections.Generic;
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
