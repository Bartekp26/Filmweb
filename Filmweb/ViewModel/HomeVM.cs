using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.ViewModel
{
    public class HomeVM
    {
        private readonly MainVM _mainVM;

        public HomeVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
