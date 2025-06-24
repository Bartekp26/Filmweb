using Filmweb.Model;
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
    public class EditProfileVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;
        public UserM CurrentUser => _mainVM.CurrentUser;
        private UserM _editedUser;
        public UserM EditedUser
        {
            get => _editedUser;
            set
            {
                _editedUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand NavigateToProfileCommand => _mainVM.NavigateToProfileCommand;

        public EditProfileVM(MainVM mainVM)
        {
            _mainVM = mainVM;

            SaveCommand = new RelayCommand(_ => SaveChanges(), _ => true);
        }

        public void editUser(UserM user)
        {
            EditedUser = new UserM
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                JoinDate = user.JoinDate,
                FavouriteMovies = user.FavouriteMovies
            };
        }

        private void SaveChanges()
        {
            try
            {
                // Walidacja
                if (string.IsNullOrWhiteSpace(EditedUser.Username) ||
                    string.IsNullOrWhiteSpace(EditedUser.FirstName) ||
                    string.IsNullOrWhiteSpace(EditedUser.LastName) ||
                    string.IsNullOrWhiteSpace(EditedUser.Email))
                {
                    MessageBox.Show("Wszystkie pola są wymagane", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SqlConnection connection = DatabaseConnection.GetConnection();

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Pobierz ID obecnego użytkownika
                string getIdQuery = "SELECT TOP 1 ID_Uzytkownika FROM UZ_Dane WHERE Email = @OldEmail";
                SqlCommand getIdCommand = new SqlCommand(getIdQuery, connection);
                getIdCommand.Parameters.AddWithValue("@OldEmail", _mainVM.CurrentUser.Email);
                object idResult = getIdCommand.ExecuteScalar();

                if (idResult == null)
                {
                    MessageBox.Show("Nie znaleziono użytkownika", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int userId = (int)idResult;

                // Sprawdź, czy login już istnieje (dla innego użytkownika)
                string checkLoginQuery = @"SELECT COUNT(*) FROM UZ_Login 
                                   WHERE Login = @Username AND ID_Uzytkownika != @UserId";
                SqlCommand checkLoginCmd = new SqlCommand(checkLoginQuery, connection);
                checkLoginCmd.Parameters.AddWithValue("@Username", EditedUser.Username);
                checkLoginCmd.Parameters.AddWithValue("@UserId", userId);

                int loginExists = (int)checkLoginCmd.ExecuteScalar();
                if (loginExists > 0)
                {
                    MessageBox.Show("Ten login jest już zajęty", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sprawdź, czy email już istnieje (dla innego użytkownika)
                string checkEmailQuery = @"SELECT COUNT(*) FROM UZ_Dane 
                                   WHERE Email = @Email AND ID_Uzytkownika != @UserId";
                SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, connection);
                checkEmailCmd.Parameters.AddWithValue("@Email", EditedUser.Email);
                checkEmailCmd.Parameters.AddWithValue("@UserId", userId);

                int emailExists = (int)checkEmailCmd.ExecuteScalar();
                if (emailExists > 0)
                {
                    MessageBox.Show("Ten email jest już zajęty", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Aktualizuj login
                string loginQuery = @"UPDATE UZ_Login 
                              SET Login = @Username 
                              WHERE ID_Uzytkownika = @UserId";
                SqlCommand loginCommand = new SqlCommand(loginQuery, connection);
                loginCommand.Parameters.AddWithValue("@Username", EditedUser.Username);
                loginCommand.Parameters.AddWithValue("@UserId", userId);
                loginCommand.ExecuteNonQuery();

                // Aktualizuj dane
                string userQuery = @"UPDATE UZ_Dane 
                             SET Imie = @FirstName, 
                                 Nazwisko = @LastName, 
                                 Email = @Email 
                             WHERE ID_Uzytkownika = @UserId";
                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@FirstName", EditedUser.FirstName);
                userCommand.Parameters.AddWithValue("@LastName", EditedUser.LastName);
                userCommand.Parameters.AddWithValue("@Email", EditedUser.Email);
                userCommand.Parameters.AddWithValue("@UserId", userId);
                userCommand.ExecuteNonQuery();

                // Aktualizuj użytkownika w VM
                _mainVM.CurrentUser = new UserM
                {
                    Username = EditedUser.Username,
                    FirstName = EditedUser.FirstName,
                    LastName = EditedUser.LastName,
                    Email = EditedUser.Email,
                    JoinDate = EditedUser.JoinDate,
                    FavouriteMovies = EditedUser.FavouriteMovies
                };

                MessageBox.Show("Dane zostały zapisane", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainVM.NavigateToProfileCommand.Execute(null);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Błąd SQL: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
