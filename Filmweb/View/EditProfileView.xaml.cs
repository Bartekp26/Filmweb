using Filmweb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Filmweb.View
{
    /// <summary>
    /// Logika interakcji dla klasy EditProfileView.xaml
    /// </summary>
    public partial class EditProfileView : UserControl
    {
        public EditProfileView()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditProfileVM vm)
            {
                PasswordBox.PasswordChanged += (s, args) => vm.Password = PasswordBox.Password;
                ConfirmPasswordBox.PasswordChanged += (s, args) => vm.ConfirmPassword = ConfirmPasswordBox.Password;

                vm.OnPasswordChangeSucceeded = () =>
                {
                    PasswordBox.Password = string.Empty;
                    ConfirmPasswordBox.Password = string.Empty;
                };
            }
        }
    }
}
