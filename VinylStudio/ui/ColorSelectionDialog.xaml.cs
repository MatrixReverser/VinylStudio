using DocumentFormat.OpenXml.Drawing;
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

namespace VinylStudio.ui
{
    /// <summary>
    /// Interaktionslogik für ColorSelectionDialog.xaml
    /// </summary>
    public partial class ColorSelectionDialog : Window
    {
        private string _htmlColor;

        public ColorSelectionDialog(string initialHTMLColor)
        {
            InitializeComponent();
            _htmlColor = initialHTMLColor;

            SolidColorBrush? brush = new BrushConverter().ConvertFromString(_htmlColor) as SolidColorBrush;
            if (brush != null ) 
            {
                colorPicker.Color = brush.Color;
            }
        }

        /**
         * Shows the dialog and returns the color when closed
         */
        public string OpenDialog()
        {
            ShowDialog();
            return _htmlColor;
        }

        /**
         * Is called if the user clicked the ok button
         */
        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            Color color = colorPicker.Color;
            _htmlColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            
            Close();
        }

        /**
         * Is called if the user clicked the cancel button
         */
        private void OnCancelClicked(object sender, RoutedEventArgs e) 
        { 
            Close();
        }
    }
}
