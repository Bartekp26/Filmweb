using Filmweb.Model;
using Filmweb.ViewModel.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class RegisterVM 
    {
        private readonly MainVM _mainVM;

        public ICommand ForwardToLoginCommand => _mainVM.SwitchToLoginCommand;
        public ICommand RegisterInCommand => _mainVM.RegisterInCommand;

       

        public RegisterVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

    // todo: poprawna walidacja

    }
}
