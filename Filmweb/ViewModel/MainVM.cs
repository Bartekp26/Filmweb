using Filmweb.View;
using Filmweb.ViewModel.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Collections;
using Filmweb.Model;
using System.Windows;

namespace Filmweb.ViewModel
{
    public class MainVM : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged(nameof(CurrentView));
                    OnPropertyChanged(nameof(IsLoginOrRegisterView));
                }
            }
        }

        public ICommand SwitchToRegisterCommand { get; }
        public ICommand SwitchToLoginCommand { get; }
        public ICommand LogInCommand { get; }
        public ICommand RegisterInCommand { get; }
        public ICommand LogOutCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToEditProfileCommand { get; }

        public bool IsLoginOrRegisterView => CurrentView is LoginView || CurrentView is RegisterView;

        private readonly LoginView _loginView;
        private readonly RegisterView _registerView;
        private readonly HomeView _homeView;
        private readonly ProfileView _profileView;
        private readonly EditProfileView _editprofileView;
        private RegisterVM _registerVM;

        public MainVM()
        {
            _registerVM = new RegisterVM(this);
            var loginVM = new LoginVM(this);
            var registerVM = new RegisterVM(this);
            var homeVM = new HomeVM(this);
            var profileVM = new ProfileVM(this);
            var editprofileVM = new EditProfileVM(this);

            _loginView = new LoginView { DataContext = loginVM };
            _registerView = new RegisterView { DataContext = registerVM };
            _homeView = new HomeView { DataContext = homeVM };
            _profileView = new ProfileView { DataContext = profileVM };
            _editprofileView = new EditProfileView { DataContext = editprofileVM };

            SwitchToRegisterCommand = new RelayCommand(_ => CurrentView = _registerView, p => true);
            SwitchToLoginCommand = new RelayCommand(_ => CurrentView = _loginView, p => true);

            LogInCommand = new RelayCommand(_ => ExecuteLogin(), p => true);
            RegisterInCommand = new RelayCommand(_ => ExecuteRegister(), p => true);
            LogOutCommand = new RelayCommand(_ => ExecuteLogout(), p => true);

            NavigateToHomeCommand = new RelayCommand(_ => CurrentView = _homeView, p => true);
            NavigateToEditProfileCommand = new RelayCommand(_ => CurrentView = _editprofileView, p => true);
            NavigateToProfileCommand = new RelayCommand(_ => CurrentView = _profileView, p => true);

            CurrentView = _loginView;
        }


/*        public void dc()
        {
            SqlConnection connection = DatabaseConnection.GetConnection();
            try
            {
                string query = "SELECT ID_Gatunku, Gatunek FROM Gatunek";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("ID_Gatunku\tGatunek");
                    Console.WriteLine("-----------------------------");

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string nazwa = reader.GetString(1);

                        Console.WriteLine($"{id}\t\t{nazwa}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
            }
        }*/
        private void ExecuteLogin()
        {
            //dc();
            CurrentView = _homeView;
        }

        public void ExecuteRegister()
        {
            
            CurrentView = _homeView;
            // po tej rejestracji lub logowaniu takie cos trza jebnac pobierajac te rzeczy z bazy, tak samo przy edycji
            //UserM user = new UserM
            //{
            //    FirstName = FirstName,
            //    LastName = LastName,
            //    Email = Email,
            //    Username = Username,
            //    Password = Password,
            //    JoinDate = JoinDate
            //};
        }


        private void ExecuteLogout()
        {
            CurrentView = _loginView;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
