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
    /// Logika interakcji dla klasy HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private readonly string PlaceholderText = "Wyszukaj...";

        public HomeView()
        {
            InitializeComponent();
        }
        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is HomeVM vm)
                vm.GoToPreviousPage();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is HomeVM vm)
                vm.GoToNextPage();
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == PlaceholderText)
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = PlaceholderText;
                SearchBox.Foreground = Brushes.Gray;
            }
        }
    }
}
