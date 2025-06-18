using Filmweb.Model;
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
    /// Logika interakcji dla klasy UserControl1.xaml
    /// </summary>
    public partial class MovieItemView : UserControl
    {
        public MovieItemView()
        {
            InitializeComponent();



        }
        private void Zobacz_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MovieListItemM movie)
            {
                var homeVM = FindHomeVM();
                homeVM?.OpenMovieDetails(movie.Title);
            }
        }

        private HomeVM FindHomeVM()
        {
            DependencyObject parent = this;
            while (parent != null)
            {
                if ((parent as FrameworkElement)?.DataContext is HomeVM vm)
                    return vm;

                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
