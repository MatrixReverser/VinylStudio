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
using VinylStudio.model;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für GenreEditDialog.xaml
    /// </summary>
    public partial class GenreEditDialog : Window
    {
        private GenreModel? _model = null;

        /**
         * Constructor of this class
         */
        public GenreEditDialog()
        {
            InitializeComponent();
        }

        /**
         * Shows this dialog and return a Genre if the user clicked "save". Otherwise null is returned
         */
        internal GenreModel? OpenDialog()
        {
            ShowDialog();
            return _model;
        }

        /**
         * Is called if the user clicked Save
         */
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            string name = textboxName.Text.Trim();

            if (name != string.Empty)
            {
                _model = new()
                {
                    Name = name
                };
            }

            Close();
        }

        /**
         * Is called if the user clicked Cancel
         */
        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
