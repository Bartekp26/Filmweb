using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class AddReviewVM
    {
        private readonly MainVM _mainVM;
        public AddReviewVM(MainVM mainVM) 
        {
            _mainVM = mainVM;
        }   
    }
}
