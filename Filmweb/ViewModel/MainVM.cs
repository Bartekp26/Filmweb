﻿using Filmweb.View;
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
        public ICommand AddReviewCommand { get; }

        public bool IsLoginOrRegisterView => CurrentView is LoginView || CurrentView is RegisterView;

        private readonly LoginView _loginView;
        private readonly RegisterView _registerView;
        private readonly HomeView _homeView;
        private readonly ProfileView _profileView;
        private readonly EditProfileView _editprofileView;
        private readonly AddReviewView _addreviewView;
        private RegisterVM _registerVM;
        private EditProfileVM _editProfileVM;

        private UserM _currentUser;
        public UserM CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public MainVM()
        {
            _registerVM = new RegisterVM(this);
            _editProfileVM = new EditProfileVM(this);
            var loginVM = new LoginVM(this);
            var registerVM = new RegisterVM(this);
            var homeVM = new HomeVM(this);
            var profileVM = new ProfileVM(this);
            var editprofileVM = new EditProfileVM(this);
            var addreviewVM = new AddReviewVM(this);

            _loginView = new LoginView { DataContext = loginVM };
            _registerView = new RegisterView { DataContext = registerVM };
            _homeView = new HomeView { DataContext = homeVM };
            _profileView = new ProfileView { DataContext = profileVM };
            _addreviewView = new AddReviewView { DataContext = addreviewVM };
            _editprofileView = new EditProfileView { DataContext = _editProfileVM };

            SwitchToRegisterCommand = new RelayCommand(_ => CurrentView = _registerView, p => true);
            SwitchToLoginCommand = new RelayCommand(_ => CurrentView = _loginView, p => true);

            LogInCommand = new RelayCommand(_ => ExecuteLogin(), p => true);
            RegisterInCommand = new RelayCommand(_ => ExecuteRegister(), p => true);
            LogOutCommand = new RelayCommand(_ => ExecuteLogout(), p => true);

            NavigateToHomeCommand = new RelayCommand(_ => CurrentView = _homeView, p => true);
            NavigateToEditProfileCommand = new RelayCommand(_ => CurrentView = _editprofileView, p => true);
            NavigateToProfileCommand = new RelayCommand(_ => CurrentView = _profileView, p => true);
            AddReviewCommand = new RelayCommand(_ => CurrentView = _addreviewView, p => true);

            CurrentView = _loginView;
        }

        private void ExecuteLogin()
        {
            var loginVM = _loginView.DataContext as LoginVM;
            if (loginVM == null) return;

            if (!loginVM.TryLoginUser(out var errorMessage, out var user))
            {
                MessageBox.Show(errorMessage, "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentUser = user;
            _editProfileVM.editUser(CurrentUser);
            loginVM.ClearAllFields(() => (_loginView as LoginView)?.ClearPasswordBox());
            CurrentView = _homeView;
        }

        public void NavigateToMovieDetails(string title)
        {
            var vm = new MovieDetailsVM(title, this);
            var view = new MovieDetailsView { DataContext = vm };
            CurrentView = view;
        }
        public void ExecuteRegister()
        {
            var registerVM = _registerView.DataContext as RegisterVM;

            if (registerVM == null) return;

            if (!registerVM.TryRegisterUser(out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            registerVM.ClearAllFields(() => (_registerView as RegisterView)?.ClearPasswordBox());
            CurrentView = _loginView;
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
