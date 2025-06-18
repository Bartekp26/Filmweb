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
using System.Windows.Input;
using BCrypt.Net;

namespace Filmweb.ViewModel
{
    public class RegisterVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;

        private string _firstName;
        private string _lastName;
        private string _email;
        private string _username;
        private string _password;

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand ForwardToLoginCommand => _mainVM.SwitchToLoginCommand;
        public ICommand RegisterInCommand => _mainVM.RegisterInCommand;

        public RegisterVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        public bool TryRegisterUser(out string errorMessage)
        {
            errorMessage = ValidateRegister();

            if (!string.IsNullOrEmpty(errorMessage))
                return false;

            try
            {
                return SaveUserToDatabase(out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd rejestracji: {ex.Message}";
                return false;
            }
        }

        private bool SaveUserToDatabase(out string errorMessage)
        {
            errorMessage = null;
            SqlConnection connection = DatabaseConnection.GetConnection();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Wstaw dane użytkownika
                    string userQuery = @"INSERT INTO UZ_Dane (Imie, Nazwisko, Email, Data_dolaczenia) 
                       VALUES (@FirstName, @LastName, @Email, @JoinDate);
                       SELECT SCOPE_IDENTITY();";

                    int userId;
                    using (SqlCommand command = new SqlCommand(userQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstName);
                        command.Parameters.AddWithValue("@LastName", LastName);
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@JoinDate", DateTime.Now);

                        // Pobierz wygenerowane ID
                        userId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // 2. Wstaw dane logowania
                    string loginQuery = @"INSERT INTO UZ_Login (ID_Uzytkownika, Login, Haslo) 
                        VALUES (@UserId, @Username, @Password)";

                    using (SqlCommand command = new SqlCommand(loginQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Username", Username);
                        command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(Password));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (SqlException ex) when (ex.Number == 2627) // Duplicate key error
                {
                    transaction.Rollback();
                    errorMessage = "Nazwa użytkownika lub email jest już zajęty.";
                    return false;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public string ValidateRegister()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FirstName))
                errors.Add("Imię jest wymagane.");
            else if (FirstName.Length > 30)
                errors.Add("Imię nie może być dłuższe niż 30 znaków.");

            if (string.IsNullOrWhiteSpace(LastName))
                errors.Add("Nazwisko jest wymagane.");
            else if (LastName.Length > 30)
                errors.Add("Nazwisko nie może być dłuższe niż 30 znaków.");

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Login jest wymagany.");
            else if (Username.Length > 30)
                errors.Add("Login nie może być dłuższy niż 30 znaków.");
            else if (IsUsernameInDatabase(Username))
                errors.Add("Podana nazwa użytkownika jest już zajęta.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Hasło jest wymagane.");
            else
            {
                if (Password.Length < 6)
                    errors.Add("Hasło musi mieć co najmniej 6 znaków.");
                if (!Regex.IsMatch(Password, @"[A-Z]"))
                    errors.Add("Hasło musi zawierać co najmniej jedną dużą literę.");
                if (!Regex.IsMatch(Password, @"\d"))
                    errors.Add("Hasło musi zawierać co najmniej jedną cyfrę.");
                if (!Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}|<>]"))
                    errors.Add("Hasło musi zawierać co najmniej jeden znak specjalny.");
            }

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("E-mail jest wymagany.");
            else
            {
                if (Email.Length > 30)
                    errors.Add("E-mail nie może być dłuższy niż 30 znaków.");
                if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    errors.Add("Nieprawidłowy adres e-mail.");
                else if (IsEmailInDatabase(Email))
                    errors.Add("Podany email jest już zajęty.");
            }

            return string.Join("\n", errors);
        }
        private bool IsUsernameInDatabase(string username)
        {
            SqlConnection connection = DatabaseConnection.GetConnection();

            try
            {
                string query = $"SELECT COUNT(*) FROM UZ_Login WHERE Login = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();
                    return count == 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas sprawdzania unikalności: {ex.Message}");
                return false;
            }
        }

        private bool IsEmailInDatabase(string email)
        {
            SqlConnection connection = DatabaseConnection.GetConnection();

            try
            {
                string query = $"SELECT COUNT(*) FROM UZ_Dane WHERE Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = (int)command.ExecuteScalar();
                    return count == 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas sprawdzania unikalności: {ex.Message}");
                return false;
            }
        }
        public void ClearAllFields(Action clearPasswordBoxAction = null)
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            clearPasswordBoxAction?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
