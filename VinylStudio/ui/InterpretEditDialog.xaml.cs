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
using VinylStudio.util;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für InterpretEditDialog.xaml
    /// </summary>
    public partial class InterpretEditDialog : Window
    {
        private InterpretModel? _interpret = null;

        /**
         * Constructor of this class
         */
        public InterpretEditDialog()
        {
            InitializeComponent();

            // fill the country combo box with the names of all countries in this world
            comboboxCountry.ItemsSource = CountryHelper.GetCountryNames();
            comboboxCountry.SelectedItem = "Germany";
        }

        /**
         * Opens the dialog and returns the interpret, if save is clicked. Otherwise null is returned
         */
        internal InterpretModel? OpenDialog()
        {
            ShowDialog();
            return _interpret;
        }

        /**
         * Is called if the user clicked Save
         */
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            string name = textboxName.Text.Trim();
            string country = comboboxCountry.Text.Trim();

            if (name != string.Empty && country != string.Empty) 
            {
                _interpret = new()
                {
                    Name = name,
                    Country = country
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
