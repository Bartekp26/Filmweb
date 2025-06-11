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
        public ICommand LogOutCommand { get; }

        public bool IsLoginOrRegisterView => CurrentView is LoginView || CurrentView is RegisterView;

        private readonly LoginView _loginView;
        private readonly RegisterView _registerView;
        private readonly HomeView _homeView;

        public MainVM()
        {
            var loginVM = new LoginVM(this);
            var registerVM = new RegisterVM(this);
            var homeVM = new HomeVM(this);

            _loginView = new LoginView { DataContext = loginVM };
            _registerView = new RegisterView { DataContext = registerVM };
            _homeView = new HomeView { DataContext = homeVM };

            SwitchToRegisterCommand = new RelayCommand(_ => CurrentView = _registerView, p => true);
            SwitchToLoginCommand = new RelayCommand(_ => CurrentView = _loginView, p => true);

            LogInCommand = new RelayCommand(_ => ExecuteLogin(), p => true);
            LogOutCommand = new RelayCommand(_ => ExecuteLogout(), p => true);

            CurrentView = _loginView;
        }

        private void ExecuteLogin()
        {
            CurrentView = _homeView;
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
