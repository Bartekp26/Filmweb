using Filmweb.Model;
using Filmweb.ViewModel.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }
        public Action OnPasswordChangeSucceeded { get; set; }

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
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(EditedUser.FirstName))
                    errors.Add("Imię jest wymagane.");
                else if (EditedUser.FirstName.Length > 30)
                    errors.Add("Imię nie może być dłuższe niż 30 znaków.");

                if (string.IsNullOrWhiteSpace(EditedUser.LastName))
                    errors.Add("Nazwisko jest wymagane.");
                else if (EditedUser.LastName.Length > 30)
                    errors.Add("Nazwisko nie może być dłuższe niż 30 znaków.");

                if (string.IsNullOrWhiteSpace(EditedUser.Username))
                    errors.Add("Login jest wymagany.");
                else if (EditedUser.Username.Length > 30)
                    errors.Add("Login nie może być dłuższy niż 30 znaków.");

                if (string.IsNullOrWhiteSpace(EditedUser.Email))
                    errors.Add("E-mail jest wymagany.");
                else
                {
                    if (EditedUser.Email.Length > 30)
                        errors.Add("E-mail nie może być dłuższy niż 30 znaków.");
                    if (!Regex.IsMatch(EditedUser.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        errors.Add("Nieprawidłowy adres e-mail.");
                }

                if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    if (string.IsNullOrWhiteSpace(Password))
                        errors.Add("Hasło jest wymagane.");
                    if (Password.Length < 6)
                        errors.Add("Hasło musi mieć co najmniej 6 znaków.");
                    if (!Regex.IsMatch(Password, @"[A-Z]"))
                        errors.Add("Hasło musi zawierać co najmniej jedną dużą literę.");
                    if (!Regex.IsMatch(Password, @"\d"))
                        errors.Add("Hasło musi zawierać co najmniej jedną cyfrę.");
                    if (!Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}|<>]"))
                        errors.Add("Hasło musi zawierać co najmniej jeden znak specjalny.");
                    if (Password != ConfirmPassword)
                        errors.Add("Hasła się nie zgadzają.");
                }

                if (errors.Any())
                {
                    string message = string.Join("\n", errors);
                    MessageBox.Show(message, "Błędy walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SqlConnection connection = DatabaseConnection.GetConnection();

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

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

                string loginQuery = @"UPDATE UZ_Login 
                              SET Login = @Username 
                              WHERE ID_Uzytkownika = @UserId";
                SqlCommand loginCommand = new SqlCommand(loginQuery, connection);
                loginCommand.Parameters.AddWithValue("@Username", EditedUser.Username);
                loginCommand.Parameters.AddWithValue("@UserId", userId);
                loginCommand.ExecuteNonQuery();

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

                if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    string updatePasswordQuery = @"UPDATE UZ_Login 
                                   SET Haslo = @Password, 
                                       LastModified = GETDATE()
                                   WHERE ID_Uzytkownika = @UserId";
                    SqlCommand passwordCommand = new SqlCommand(updatePasswordQuery, connection);
                    passwordCommand.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(Password));
                    passwordCommand.Parameters.AddWithValue("@UserId", userId);
                    passwordCommand.ExecuteNonQuery();
                }

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
                OnPasswordChangeSucceeded?.Invoke();
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
