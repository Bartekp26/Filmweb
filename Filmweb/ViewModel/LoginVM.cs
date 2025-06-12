using Filmweb.ViewModel.BaseClass;
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
    public class LoginVM
    {
        private readonly MainVM _mainVM;

        public ICommand SwitchToRegisterCommand => _mainVM.SwitchToRegisterCommand;
        public ICommand LogInCommand => _mainVM.LogInCommand;

        public LoginVM(MainVM mainVM)
        {
            
            _mainVM = mainVM;
        }
    }
}
