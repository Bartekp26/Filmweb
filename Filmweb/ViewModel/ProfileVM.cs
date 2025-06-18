using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
