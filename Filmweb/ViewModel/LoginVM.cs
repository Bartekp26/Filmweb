﻿using Filmweb.Model;
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
    public class LoginVM : ViewModelBase
    {
        private readonly MainVM _mainVM;

        private string _username;
        private string _password;

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

        public ICommand SwitchToRegisterCommand => _mainVM.SwitchToRegisterCommand;
        public ICommand LogInCommand => _mainVM.LogInCommand;

        public LoginVM(MainVM mainVM)
        {
            
            _mainVM = mainVM;
        }

        public bool TryLoginUser(out string errorMessage, out UserM loggedInUser)
        {
            errorMessage = null;
            loggedInUser = null;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                errorMessage = "Wprowadź login i hasło";
                return false;
            }

            try
            {
                var connection = DatabaseConnection.GetConnection();
                
                    string query = @"SELECT UD.Imie, UD.Nazwisko, UD.Email, 
                               UD.Data_dolaczenia, UL.Login, UL.Haslo
                               FROM UZ_Login UL
                               JOIN UZ_Dane UD ON UL.ID_Uzytkownika = UD.ID_Uzytkownika
                               WHERE UL.Login = @Username";

                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", Username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            errorMessage = "Nieprawidłowy login lub hasło";
                            return false;
                        }

                        string storedHash = reader["Haslo"].ToString();
                        if (!BCrypt.Net.BCrypt.Verify(Password, storedHash))
                        {
                            errorMessage = "Nieprawidłowy login lub hasło";
                            return false;
                        }

                        loggedInUser = new UserM
                        {
                            FirstName = reader["Imie"].ToString(),
                            LastName = reader["Nazwisko"].ToString(),
                            Email = reader["Email"].ToString(),
                            JoinDate = Convert.ToDateTime(reader["Data_dolaczenia"]),
                            Username = reader["Login"].ToString(),
                        };

                    return true;
                    }
                
            }
            catch (Exception ex)
            {
                errorMessage = "Błąd podczas logowania";
                Console.WriteLine($"Błąd: {ex.Message}");
                return false;
            }
        }

        public void ClearAllFields(Action clearPasswordBoxAction = null)
        {
            Username = string.Empty;
            Password = string.Empty;
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            clearPasswordBoxAction?.Invoke();
        }
    }
}
