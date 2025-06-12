using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class EditProfileVM
    {
        private readonly MainVM _mainVM;

        public ICommand NavigateToProfileCommand => _mainVM.NavigateToProfileCommand;

        public EditProfileVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
