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
using System.Windows.Shapes;

namespace Filmweb.View
{
    /// <summary>
    /// Logika interakcji dla klasy AddReviewView.xaml
    /// </summary>
    public partial class AddReviewView : UserControl
    {
        public AddReviewView()
        {
            InitializeComponent();
        }

        private void IntegerUpDown_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown intUpDown && intUpDown.Value.HasValue)
            {
                int value = intUpDown.Value.Value;

                if (value < intUpDown.Minimum)
                    intUpDown.Value = (int)intUpDown.Minimum;
                else if (value > intUpDown.Maximum)
                    intUpDown.Value = (int)intUpDown.Maximum;
            }
        }

        private void IntegerUpDown_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!int.TryParse(text, out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
